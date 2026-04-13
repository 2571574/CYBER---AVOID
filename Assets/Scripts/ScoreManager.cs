using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private GameStatus settings;

    // 現在のスコア（内部的には正確な計算のためにfloatを使用し、表示時にintにします）
    public float CurrentScore { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.Playing)
        {
            CurrentScore = 0f;
        }
    }

    private void Update()
    {
        // プレイ中のみ、時間経過でスコアを加算
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (settings != null)
        {
            CurrentScore += settings.scorePerSecond * Time.deltaTime;
        }
    }

    // ギリギリで回避した時などに外部から呼び出すメソッド（後で弾の処理に組み込みます）
    public void AddDodgeBonus()
    {
        if (settings != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            CurrentScore += settings.dodgeBonusScore;
            Debug.Log("ボーナス獲得！");

            if (UIManager.Instance != null) {
                UIManager.Instance.ShowBonusText(Mathf.FloorToInt(settings.dodgeBonusScore));
            }
        }
    }
}