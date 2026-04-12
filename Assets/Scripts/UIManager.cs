using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // TextMeshProを使用する場合は TMPro に変更してください

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

    [Header("Transition Elements")]
    [Tooltip("画面全体を覆う黒いUI（CanvasGroup付き）")]
    [SerializeField] private CanvasGroup fadePanelGroup;
    [Tooltip("「START」などのテキスト")]
    [SerializeField] private TextMeshProUGUI startPresentationText;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

        // 修正：Ready（入場演出中）、Playing（本編）、PlayerDead（死亡演出中）の時にHUDを表示する
        if (hudPanel != null)
        {
            hudPanel.SetActive(state == GameState.StartAnim ||
                                state == GameState.CharaReady ||
                                state == GameState.Playing ||
                                state == GameState.PlayerDead);
        }

        gameOverPanel.SetActive(state == GameState.GameOver);

        if (touchInputPanel != null)
        {
            // 操作用パネルは「Playing」中のみアクティブにする
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

    public IEnumerator FadeOutRoutine(float duration)
    {
        if (fadePanelGroup == null) yield break;
        fadePanelGroup.gameObject.SetActive(true);
        fadePanelGroup.blocksRaycasts = true;

        fadePanelGroup.transform.SetAsLastSibling();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        fadePanelGroup.alpha = 1f;
    }

    public IEnumerator FadeInRoutine(float duration)
    {
        if (fadePanelGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanelGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        fadePanelGroup.alpha = 0f;
        fadePanelGroup.blocksRaycasts = false;
        fadePanelGroup.gameObject.SetActive(false);
    }

    // キャラクターの入場コルーチン
    public IEnumerator ReadyPresentationRoutine(Transform playerTransform, Vector2 targetPosition)
    {
        Vector2 startPosition = playerTransform != null ? (Vector2)playerTransform.position : targetPosition;
        startPosition.y = targetPosition.y;

        if (playerTransform != null)
        {
            float speed = GameManager.Instance.InitialScrollSpeed;
            float distance = Vector2.Distance(startPosition, targetPosition);
            float playerDuration = distance / speed;

            // ▼ 修正：テキストの演出時間をプレイヤーの移動時間のちょうど「2倍」にする
            float textDuration = playerDuration * 2.0f;
            StartCoroutine(StartTextPresentationRoutine(textDuration));

            float elapsed = 0f;
            while (elapsed < playerDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / playerDuration;

                playerTransform.position = Vector2.Lerp(startPosition, targetPosition, t);

                yield return null;
            }

            playerTransform.position = targetPosition;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }
    }

    // テキスト専用の独立したアニメーションコルーチン
    private IEnumerator StartTextPresentationRoutine(float textDuration)
    {
        float elapsed = 0f;
        RectTransform textRect = null;
        Vector2 textStartPos = Vector2.zero;
        Vector2 textEndPos = Vector2.zero;

        if (startPresentationText != null)
        {
            startPresentationText.gameObject.SetActive(true);
            Color initialColor = startPresentationText.color;
            initialColor.a = 1f;
            startPresentationText.color = initialColor;

            textRect = startPresentationText.rectTransform;
            float screenWidthOffset = 1500f;
            float currentY = textRect.anchoredPosition.y;

            textStartPos = new Vector2(screenWidthOffset, currentY);
            textEndPos = new Vector2(-screenWidthOffset, currentY);
        }

        while (elapsed < textDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textDuration;

            if (textRect != null)
            {
                float normalizedT = t - 0.5f;
                float textEaseT = 4.0f * Mathf.Pow(normalizedT, 3f) + 0.5f;

                textRect.anchoredPosition = Vector2.Lerp(textStartPos, textEndPos, textEaseT);
            }

            yield return null;
        }

        if (startPresentationText != null)
        {
            startPresentationText.gameObject.SetActive(false);
            if (textRect != null) textRect.anchoredPosition = new Vector2(0, textRect.anchoredPosition.y);
        }
    }

    public IEnumerator WaitTextExitRoutine()
    {
        while (startPresentationText != null && startPresentationText.gameObject.activeSelf)
        {
            yield return null;
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