using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

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

    [Header("Sub Systems (Managers)")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private EffectManager effectManager;
    [SerializeField] private RankingManager rankingManager;
    [SerializeField] private LevelManager levelManager;
    public UIManager UI => uiManager;
    public AudioManager Audio => audioManager;
    public ScoreManager Score => scoreManager;
    public EffectManager Effect => effectManager;
    public RankingManager Ranking => rankingManager;
    public LevelManager Level => levelManager;

    // 現在のゲームステート
    public GameState CurrentState { get; private set; }

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
        if (playerTransform != null)
        {
            var playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null) playerController.OnJumped += HandlePlayerJump;

            var detectDodge = playerTransform.GetComponentInChildren<DetectDodge>();
            if (detectDodge != null) detectDodge.OnDodged += HandlePlayerDodge;

            var playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDamaged += HandlePlayerDamage;
                playerHealth.OnPlayerDeadEvent += HandlePlayerDeathTriggered;
            }
            var bulletSpawner = FindObjectOfType<BulletSpawner>();
            if (bulletSpawner != null)
            {
                bulletSpawner.OnBulletAlert += HandleBulletAlert;
            }

            if (uiManager != null)
            {
                uiManager.OnStartRequested += HandleStartRequested;
                uiManager.OnRetryRequested += HandleRetryRequested;
                uiManager.OnTitleRequested += GoToTitle;
                uiManager.OnRankingOpenRequested += HandleRankingOpenRequested;
                uiManager.OnUIActionSoundRequested += HandleUISound;
            }
        }
        ChangeState(GameState.Title);
    }

    private void OnDestroy()
    {
        if (playerTransform != null)
        {
            var playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null) playerController.OnJumped -= HandlePlayerJump;

            var detectDodge = playerTransform.GetComponentInChildren<DetectDodge>();
            if (detectDodge != null) detectDodge.OnDodged -= HandlePlayerDodge;

            var playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDamaged -= HandlePlayerDamage;
                playerHealth.OnPlayerDeadEvent -= HandlePlayerDeathTriggered;
            }
        }

        var bulletSpawner = FindObjectOfType<BulletSpawner>();
        if (bulletSpawner != null)
        {
            bulletSpawner.OnBulletAlert -= HandleBulletAlert;
        }

        if (uiManager != null)
        {
            uiManager.OnStartRequested -= HandleStartRequested;
            uiManager.OnRetryRequested -= HandleRetryRequested;
            uiManager.OnTitleRequested -= GoToTitle;
            uiManager.OnRankingOpenRequested -= HandleRankingOpenRequested;
            uiManager.OnUIActionSoundRequested -= HandleUISound;
        }
    }

    private void HandlePlayerJump()
    {
        if (audioManager != null) audioManager.PlaySE(SEType.Jump);
    }

    private void HandlePlayerDodge(Vector3 position)
    {
        int bonusValue = 0;
        if (scoreManager != null) bonusValue = scoreManager.AddDodgeBonus();
        if (audioManager != null) audioManager.PlaySE(SEType.Dodge);
        if (uiManager != null && bonusValue > 0)
        {
            uiManager.ShowBonusText(bonusValue);
        }
        if (effectManager != null) effectManager.PlayScoreEffect(position);
    }

    private void HandlePlayerDamage(Vector3 position, float shakeDuration, float shakeMagnitude)
    {
        if (audioManager != null) audioManager.PlaySE(SEType.Damage);
        if (effectManager != null) effectManager.PlayDamageEffect(position);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
    }

    private void HandlePlayerDeathTriggered(Vector3 position, float shakeDuration, float shakeMagnitude)
    {
        if (audioManager != null) audioManager.PlaySE(SEType.Death);
        if (effectManager != null) effectManager.PlayDeathEffect(position);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        StartCoroutine(PlayerDeathSequence());
    }
    private void HandleBulletAlert()
    {
        if (audioManager != null) audioManager.PlaySE(SEType.Alert);
    }

    private void HandleUISound(SEType seType)
    {
        if (audioManager != null) audioManager.PlaySE(seType);
    }

    private async void HandleStartRequested(string playerName)
    {
        await PreparePlayerSessionAsync(playerName);
        StartGame();
    }

    private async void HandleRetryRequested(string playerName)
    {
        await PreparePlayerSessionAsync(playerName);
        RetryGame();
    }

    private async Task PreparePlayerSessionAsync(string playerName)
    {
        if (rankingManager != null)
        {
            await rankingManager.ResetPlayerSessionAsync();
            await rankingManager.UpdatePlayerNameAsync(playerName);
        }
    }

    /// <summary>
    /// ランキングの読み込み処理
    /// </summary>
    private async void HandleRankingOpenRequested()
    {
        if (rankingManager != null && uiManager != null)
        {
            //ローディング表示
            uiManager.ShowRankingLoading(false);
            //ランキングの取得
            bool isSuccess = await rankingManager.FetchRankingAsync();

            uiManager.UpdateRankingDisplay(false, !isSuccess);
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
            if (audioManager != null) audioManager.PlayBGM(BGMType.Title);
        }
        else if (newState == GameState.StartAnim)
        {
            if (audioManager != null) audioManager.PlayBGM(BGMType.Play);
        }
        else if (newState == GameState.PlayerDead)
        {
            if (audioManager != null) audioManager.StopBGM();
        }
        else if (newState == GameState.GameOver)
        {
            ProcessGameOverRankingAsync();
        }

        OnStateChanged?.Invoke(newState);
    }

    private async void ProcessGameOverRankingAsync()
    {
        if (scoreManager == null || rankingManager == null || uiManager == null) return;

        int finalScore = Mathf.FloorToInt(scoreManager.CurrentScore);
        uiManager.SetResultScoreText(finalScore);
        uiManager.ShowRankingLoading(true);

        await rankingManager.AddScoreAndSaveAsync(finalScore);
        await Task.Delay(1000);

        if (this == null || CurrentState != GameState.GameOver) return;

        bool isSuccess = await rankingManager.FetchRankingAsync();

        if (this == null || CurrentState != GameState.GameOver) return;

        uiManager.UpdateRankingDisplay(true, !isSuccess);
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


    // コルーチンによるシーケンス処理

    private IEnumerator StartGameSequence()
    {
        isTransitioning = true;

        yield return StartCoroutine(GameManager.Instance.UI.FadeOutRoutine(0.5f));

        ChangeState(GameState.StartAnim);
        if (levelManager != null) levelManager.ResetStatus();
        ClearField();

        if (playerTransform != null)
        {
            playerTransform.position = playerStartSpawnPosition;
            playerTransform.gameObject.SetActive(true);
        }

        yield return StartCoroutine(GameManager.Instance.UI.FadeInRoutine(0.5f));

        yield return StartCoroutine(GameManager.Instance.UI.ReadyPresentationRoutine(playerTransform, playerReadyPosition));

        
        ChangeState(GameState.CharaReady);

        yield return StartCoroutine(GameManager.Instance.UI.WaitTextExitRoutine());

        ChangeState(GameState.Playing);

        isTransitioning = false; 
    }

    private IEnumerator PlayerDeathSequence()
    {
        isTransitioning = true;
        ChangeState(GameState.PlayerDead);

        float elapsed = 0f;
        while (elapsed < deathWaitTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathWaitTime);
            float easeOutT = 1f - Mathf.Pow(1f - t, 3f);

            // 修正: スクロール倍率の変更をLevelManagerに依頼
            if (levelManager != null)
                levelManager.SetGlobalScrollMultiplier(Mathf.Lerp(1.0f, 0.0f, easeOutT));

            yield return null;
        }

        if (levelManager != null)
            levelManager.SetGlobalScrollMultiplier(0.0f);

        yield return StartCoroutine(GameManager.Instance.UI.FadeOutRoutine(1.0f));

        ChangeState(GameState.GameOver);
        yield return StartCoroutine(GameManager.Instance.UI.FadeInRoutine(0.5f));

        isTransitioning = false;
    }

    private IEnumerator GoToTitleSequence()
    {
        isTransitioning = true;

        yield return StartCoroutine(GameManager.Instance.UI.FadeOutRoutine(0.5f));

        if (levelManager != null) levelManager.ResetStatus();
        ClearField();
        ChangeState(GameState.Title);

        yield return StartCoroutine(GameManager.Instance.UI.FadeInRoutine(0.5f));

        isTransitioning = false;
    }


}