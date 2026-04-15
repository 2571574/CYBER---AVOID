using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ゲームの進行ステート
/// </summary>
public enum GameState
{
    Title,      // タイトル
    StartAnim,  // スタート演出中
    CharaReady, // キャラ演出終了、テキスト演出中
    Playing,    // ゲーム内
    PlayerDead, // プレイヤーの死亡エフェクト中
    GameOver    // リザルト画面
}

/// <summary>
/// ゲーム全体の流れを管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 現在のゲームステート
    public GameState CurrentState { get; private set; }

    // 時間経過で上昇する時の難易度の倍率
    public float DifficultyMultiplier { get; private set; } = 1.0f;

    //全体スクロールの倍率(死亡アニメーション時などに使用
    public float GlobalScrollMultiplier { get; private set; } = 1.0f;

    [Tooltip("ゲーム内のパラメータ")]
    [SerializeField] private GameStatus settings;

    //現在の基準のスクロール速度を外部に送るプロパティ
    public float InitialScrollSpeed => settings != null ? settings.scrollSpeed : 8.0f;

    [Header("Presentation Settings")]
    [Tooltip("死亡エフェクトの待機秒数")]
    [SerializeField] private float deathWaitTime = 1.5f;
    [Tooltip("スタート時のキャラクターの初期位置(カメラ外からの入場)")]
    [SerializeField] private Vector2 playerStartSpawnPosition = new Vector2(-5f, 0f);
    [Tooltip("ゲームスタート位置")]
    [SerializeField] private Vector2 playerReadyPosition = new Vector2(0f, 0f);
    [Tooltip("操作するプレイヤーキャラクターのTransform")]
    [SerializeField] private Transform playerTransform;

    /// <summary>
    /// ステートが変化した時のイベント
    /// </summary>
    public event Action<GameState> OnStateChanged;

    //プレイ時間(1ゲーム内)
    private float playTimer;

    //シーン遷移中のロックフラグ
    private bool isTransitioning = false;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        ChangeState(GameState.Title);
    }


    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            playTimer += Time.deltaTime;
            if (settings != null)
            {
                //IncreaseIntervalごとにIncreaseRate分だけ難易度倍率を上げる
                DifficultyMultiplier = 1.0f + Mathf.Max(0f, (playTimer / settings.difficultyIncreaseInterval) * settings.difficultyIncreaseRate);
            }
        }
    }

    /// <summary>
    /// ステートを変更し、イベントを発行して他のスクリプトに知らせる
    /// </summary>
    /// <param name="newState">変更後のステート</param>
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

    /// <summary>
    /// ゲームの難易度と経過時間を初期状態に戻す
    /// </summary>
    private void ResetStatus()
    {
        playTimer = 0f;
        DifficultyMultiplier = 1.0f;
        GlobalScrollMultiplier = 1.0f;
    }

    /// <summary>
    /// フィールド上のオブジェクトを全てプールに戻す
    /// </summary>
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

    /// <summary>
    /// 現在のカメラのアスペクト比から左右の端の座標を計算する
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDynamicScreenRange()
    {
        if (Camera.main != null)
        {
            float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            return new Vector2(-halfWidth + 0.15f, halfWidth - 0.15f);
        }
        return new Vector2(-2.8f, 2.8f);
    }

    // シーン遷移を外部から呼ぶための関数

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

    // コルーチンによるシーケンス処理

    private IEnumerator StartGameSequence()
    {
        isTransitioning = true;

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

        
        ChangeState(GameState.CharaReady);
        yield return StartCoroutine(UIManager.Instance.WaitTextExitRoutine());

        ChangeState(GameState.Playing);

        isTransitioning = false; 
    }

    private IEnumerator PlayerDeathSequence()
    {
        isTransitioning = true;

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

        isTransitioning = false;
    }

    private IEnumerator GoToTitleSequence()
    {
        isTransitioning = true;

        yield return StartCoroutine(UIManager.Instance.FadeOutRoutine(0.5f));

        ResetStatus();
        ClearField();
        ChangeState(GameState.Title);

        yield return StartCoroutine(UIManager.Instance.FadeInRoutine(0.5f));

        isTransitioning = false;
    }


}