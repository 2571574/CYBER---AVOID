using TMPro;
using UnityEngine;
using UnityEngine.UI; // TextMeshProを使用する場合は TMPro に変更してください

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject touchInputPanel;
    [SerializeField] private GameObject rankingPanel;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("GameOver Elements")]
    [SerializeField] private TextMeshProUGUI resultScoreText;

    [SerializeField] private TextMeshProUGUI titleRankingText;
    [SerializeField] private TextMeshProUGUI gameOverRankingText;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private void Start()
    {
        // 全員の準備が終わるStartのタイミングで登録する
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;

            // ついでに、起動時の状態（タイトル画面）を強制的にUIに反映させる
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(playerHealth.CurrentHealth);
        }

        if (rankingPanel != null) rankingPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        // 状態に合わせてパネルの表示/非表示を切り替える
        titlePanel.SetActive(state == GameState.Title);
        hudPanel.SetActive(state == GameState.Playing);
        gameOverPanel.SetActive(state == GameState.GameOver);

        // 追加: プレイ中のみ、操作用の透明パネルをオンにする
        if (touchInputPanel != null)
        {
            touchInputPanel.SetActive(state == GameState.Playing);
        }

        if (state == GameState.GameOver)
        {
            int finalScore = Mathf.FloorToInt(ScoreManager.Instance.CurrentScore);
            if (resultScoreText != null) resultScoreText.text = "SCORE: " + finalScore.ToString();

            if (RankingManager.Instance != null)
            {
                RankingManager.Instance.AddScoreAndSave(finalScore);
            }

            UpdateRankingUITexts();
        }
    }

    private void Update()
    {
        // プレイ中のみスコア表示を毎フレーム更新
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (scoreText != null && ScoreManager.Instance != null)
            {
                scoreText.text = "SCORE: " + Mathf.FloorToInt(ScoreManager.Instance.CurrentScore).ToString();
            }
        }
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.text = "LIFE: " + currentHealth.ToString();
        }
    }

    private void UpdateRankingUITexts()
    {
        if (RankingManager.Instance == null) return;

        var highScores = RankingManager.Instance.CurrentRanking.highScores;
        string rankingString = "=== TOP 5 ===\n";

        if (highScores.Count == 0)
        {
            rankingString += "NO RECORD";
        }
        else
        {
            for (int i = 0; i < highScores.Count; i++)
            {
                // 例： "1. 1500" のように改行して追加していく
                rankingString += $"{i + 1}. {highScores[i]}\n";
            }
        }

        // 両方のテキストコンポーネントに同じ文字列をセット
        if (titleRankingText != null) titleRankingText.text = rankingString;
        if (gameOverRankingText != null) gameOverRankingText.text = rankingString;
    }

    // --- ボタンメソッド ---

    public void OnClickStartButton()
    {
        GameManager.Instance.StartGame();
    }

    public void OnClickTitleButton()
    {
        GameManager.Instance.GoToTitle();
    }

    public void OnClickRetryButton()
    {
        GameManager.Instance.RetryGame();
    }
    public void OnClickOpenRankingButton()
    {
        UpdateRankingUITexts();
        if (rankingPanel != null) rankingPanel.SetActive(true);
    }

    public void OnClickCloseRankingButton()
    {
        if (rankingPanel != null) rankingPanel.SetActive(false);
    }
}

