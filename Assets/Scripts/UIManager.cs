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
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("GameOver Elements")]
    [SerializeField] private TextMeshProUGUI resultScoreText;

    [Header("Ranking Elements")]
    [SerializeField] private Transform titleRankingContent; //タイトルのランキング
    [SerializeField] private GameObject rankingTextPrefab;  // １順位ずつ表示するためのプレハブ
    [SerializeField] private TextMeshProUGUI[] gameOverRankingTexts;

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
        titlePanel.SetActive(state == GameState.Title);
        hudPanel.SetActive(state == GameState.Playing);
        gameOverPanel.SetActive(state == GameState.GameOver);

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
                // ここで保存処理を走らせる
                RankingManager.Instance.AddScoreAndSave(finalScore);
            }

            UpdateGameOverRankingUI();
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
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            // 現在の体力よりインデックスが小さければ満タン、それ以上なら空の画像にする
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }

    private void UpdateTitleRankingUI()
    {
        if (RankingManager.Instance == null || titleRankingContent == null || rankingTextPrefab == null) return;

        // 既存のリストをクリア（重複生成を防ぐため）
        foreach (Transform child in titleRankingContent)
        {
            Destroy(child.gameObject);
        }

        var highScores = RankingManager.Instance.CurrentRanking.highScores;

        if (highScores.Count == 0)
        {
            GameObject obj = Instantiate(rankingTextPrefab, titleRankingContent);
            obj.GetComponent<TextMeshProUGUI>().text = "NO RECORD";
            return;
        }

        // 30件すべて生成
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject obj = Instantiate(rankingTextPrefab, titleRankingContent);
            obj.GetComponent<TextMeshProUGUI>().text = $"{i + 1}. {highScores[i]}";
        }
    }

    private void UpdateGameOverRankingUI()
    {
        if (RankingManager.Instance == null || gameOverRankingTexts == null || gameOverRankingTexts.Length == 0) return;

        var highScores = RankingManager.Instance.CurrentRanking.highScores;

        for (int i = 0; i < gameOverRankingTexts.Length; i++)
        {
            if (i < highScores.Count)
            {
                gameOverRankingTexts[i].text = $"{i + 1}. {highScores[i]}";
                gameOverRankingTexts[i].gameObject.SetActive(true);
            }
            else
            {
                gameOverRankingTexts[i].gameObject.SetActive(false);
            }
        }
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
        UpdateTitleRankingUI();
        if (rankingPanel != null) rankingPanel.SetActive(true);
    }

    public void OnClickCloseRankingButton()
    {
        if (rankingPanel != null) rankingPanel.SetActive(false);
    }
}