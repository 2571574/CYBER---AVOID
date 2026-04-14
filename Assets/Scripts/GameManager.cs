using System;
using System.Collections;
using UnityEngine;

public enum GameState
{
    Title,
    StartAnim,      // キャラクター入場・スタート文字演出中
    CharaReady,
    Playing,
    PlayerDead, // プレイヤー死亡・エフェクト待機中（既存オブジェクトは動く）
    GameOver    // リザルト画面
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public float DifficultyMultiplier { get; private set; } = 1.0f;
    public float GlobalScrollMultiplier { get; private set; } = 1.0f;
    [SerializeField] private GameStatus settings;

    public float InitialScrollSpeed => settings != null ? settings.scrollSpeed : 8.0f;

    // 演出用のパラメータ
    [Header("Presentation Settings")]
    [Tooltip("死亡時のエフェクトを待機する秒数")]
    [SerializeField] private float deathWaitTime = 1.5f;
    [Tooltip("キャラクター入場時のスタート位置（カメラ外）")]
    [SerializeField] private Vector2 playerStartSpawnPosition = new Vector2(-5f, 0f);
    [Tooltip("キャラクターの定位置（ゲームプレイ中の基準位置）")]
    [SerializeField] private Vector2 playerReadyPosition = new Vector2(0f, 0f);
    [Tooltip("操作するプレイヤーキャラクターのTransform")]
    [SerializeField] private Transform playerTransform;

    public event Action<GameState> OnStateChanged;
    private float playTimer;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        // 起動時は即座にタイトルへ
        ChangeState(GameState.Title);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            playTimer += Time.deltaTime;
            if (settings != null)
            {
                DifficultyMultiplier = 1.0f + Mathf.Max(0f, (playTimer / settings.difficultyIncreaseInterval) * settings.difficultyIncreaseRate);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        if (newState == GameState.Title)
        {
            playTimer = 0f;
            DifficultyMultiplier = 1.0f;
        }
        OnStateChanged?.Invoke(newState);
    }

    // --- 遷移シーケンス群 ---

    public void StartGame()
    {
        if (isTransitioning) return;
        if (CurrentState != GameState.Title && CurrentState != GameState.GameOver) return;
        StartCoroutine(StartGameSequence());
    }

    public void RetryGame()
    {
        if (isTransitioning) return;
        if (CurrentState != GameState.GameOver) return;
        StartCoroutine(StartGameSequence());
    }

    public void GoToTitle()
    {
        if (isTransitioning) return;
        if (CurrentState != GameState.GameOver) return;
        StartCoroutine(GoToTitleSequence());
    }

    public void HandlePlayerDeath()
    {
        if (CurrentState != GameState.Playing) return;
        StartCoroutine(PlayerDeathSequence());
    }

    // --- コルーチンによるシーケンス処理 ---

    private IEnumerator StartGameSequence()
    {
        isTransitioning = true; // ▼ ロック開始

        yield return StartCoroutine(UIManager.Instance.FadeOutRoutine(0.5f));

        ChangeState(GameState.StartAnim);
        ResetStatus();
        ClearField();

        if (playerTransform != null)
        {
            playerTransform.position = playerStartSpawnPosition;
            playerTransform.gameObject.SetActive(true);
        }

        yield return StartCoroutine(UIManager.Instance.FadeInRoutine(0.5f));

        yield return StartCoroutine(UIManager.Instance.ReadyPresentationRoutine(playerTransform, playerReadyPosition));

        // ▼ 修正：後半（スクロールのみを開始し、テキストが完全に消えるまで待機）
        ChangeState(GameState.CharaReady);
        yield return StartCoroutine(UIManager.Instance.WaitTextExitRoutine());

        // ▼ 本編開始（敵の出現や操作がここで有効になる）
        ChangeState(GameState.Playing);

        isTransitioning = false; // ▼ ロック解除
    }

    private IEnumerator PlayerDeathSequence()
    {
        isTransitioning = true; // ▼ ロック開始

        ChangeState(GameState.PlayerDead);

        float elapsed = 0f;
        while(elapsed < deathWaitTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathWaitTime);

            float easeOutT = 1f - Mathf.Pow(1f - t, 3f);
            GlobalScrollMultiplier = Mathf.Lerp(1.0f,0.0f,easeOutT);
            yield return null;
        }
        GlobalScrollMultiplier = 0.0f;

        yield return StartCoroutine(UIManager.Instance.FadeOutRoutine(1.0f));

        ChangeState(GameState.GameOver);
        yield return StartCoroutine(UIManager.Instance.FadeInRoutine(0.5f));

        isTransitioning = false; // ▼ ロック解除
    }

    private IEnumerator GoToTitleSequence()
    {
        isTransitioning = true; // ▼ ロック開始

        yield return StartCoroutine(UIManager.Instance.FadeOutRoutine(0.5f));

        ResetStatus();
        ClearField();
        ChangeState(GameState.Title);

        yield return StartCoroutine(UIManager.Instance.FadeInRoutine(0.5f));

        isTransitioning = false; // ▼ ロック解除
    }

    private void ResetStatus()
    {
        playTimer = 0f;
        DifficultyMultiplier = 1.0f;
        GlobalScrollMultiplier = 1.0f;
    }

    private void ClearField()
    {
        PoolableObject[] poolables = FindObjectsOfType<PoolableObject>();
        foreach (var p in poolables)
        {
            if (p != null && p.gameObject.activeInHierarchy)
            {
                p.ReleaseToPool();
            }
        }
    }

    public Vector2 GetDynamicScreenRange()
    {
        if (Camera.main != null)
        {
            float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            return new Vector2(-halfWidth + 0.15f, halfWidth - 0.15f);
        }
        return new Vector2(-2.8f, 2.8f);
    }
}