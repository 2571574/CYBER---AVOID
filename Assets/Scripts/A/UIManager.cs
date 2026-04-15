using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

/// <summary>
/// ゲーム内全てのUIを管理するクラス
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [Tooltip("タイトル画面のパネル")]
    [SerializeField] private GameObject titlePanel;
    [Tooltip("ゲームオーバー画面のパネル")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("ランキング画面のパネル")]
    [SerializeField] private GameObject rankingPanel;
    [Tooltip("ガイド画面のパネル")]
    [SerializeField] private GameObject guidePanel;

    [Tooltip("プレイ中HUDのパネル")]
    [SerializeField] private GameObject hudPanel;
    [Tooltip("フェードインのためのHUDのCanvasGroup")]
    [SerializeField] private CanvasGroup hudPanelGroup;
    [Tooltip("操作パネル")]
    [SerializeField] private GameObject touchInputPanel;
    [Tooltip("フェードインのための操作パネルのCanvasGroup")]
    [SerializeField] private CanvasGroup touchInputPanelGroup;

    [Header("HUD Elements")]
    [Tooltip("スコアを表示させるテキスト")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Tooltip("体力を表示させる画像の配列")]
    [SerializeField] private Image[] heartImages;
    [Tooltip("体力がある時のアイコン")]
    [SerializeField] private Sprite fullHeartSprite;
    [Tooltip("体力がない時のアイコン")]
    [SerializeField] private Sprite emptyHeartSprite;
    [Tooltip("回避時のボーナススコアのテキストのプレハブ")]
    [SerializeField] private GameObject bonusTextPrefab;
    [Tooltip("ボーナステキストの生成位置")]
    [SerializeField] private Transform bonusTextSpawnPoint;

    [Header("GameOver Elements")]
    [Tooltip("スコアを表示させるテキスト")]
    [SerializeField] private TextMeshProUGUI resultScoreText;

    [Header("Ranking Elements")]
    [Tooltip("タイトルでランキングを一覧で並べるオブジェクト")]
    [SerializeField] private Transform titleRankingContent;
    [Tooltip("1順位ずつ表示させるためのテキストのプレハブ")]
    [SerializeField] private GameObject rankingTextPrefab;
    [Tooltip("順位テキストの配列")]
    [SerializeField] private TextMeshProUGUI[] gameOverRankingTexts;

    [Header("Transition Elements")]
    [Tooltip("フェード用の黒いパネルのCanvasGroup")]
    [SerializeField] private CanvasGroup fadePanelGroup;
    [Tooltip("ゲーム開始時のStartのテキスト")]
    [SerializeField] private TextMeshProUGUI startPresentationText;

    [Header("References")]
    [Tooltip("PlayerHealthの参照")]
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthUI;
        }

        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    private void Update()
    {
        //プレイ中のみスコアをリアルタイムで更新する
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (scoreText != null && ScoreManager.Instance != null)
            {
                scoreText.text = "SCORE: " + Mathf.FloorToInt(ScoreManager.Instance.CurrentScore).ToString();
            }
        }
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

    /// <summary>
    /// GameManagerのステートが変化したとき、該当するパネルの表示非表示を切り替える
    /// </summary>
    /// <param name="state">変更後のステート</param>
    private void HandleStateChanged(GameState state)
    {
        //タイトル画面の表示処理
        titlePanel.SetActive(state == GameState.Title);

        //ゲーム内のみHUDと操作パネルを表示
        bool isPlaying = (state == GameState.Playing);
        if (hudPanel != null)
        {
            hudPanel.SetActive(isPlaying);
        }
        if (touchInputPanel != null)
        {
            touchInputPanel.SetActive(isPlaying);
        }

        //ゲームオーバー画面の表示処理
        gameOverPanel.SetActive(state == GameState.GameOver);

        //プレイ開始時のUIをフェードインさせる演出
        if(state == GameState.Playing)
        {
            StartCoroutine(FadeInGameplayUIRoutine(0.5f));
        }

        //ゲームオーバーのスコア表示とランキングの保存、表示
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


    /// <summary>
    /// 体力が変化したとき、体力のアイコンを切り替える
    /// </summary>
    /// <param name="currentHealth"></param>
    private void UpdateHealthUI(int currentHealth)
    {
        if (heartImages == null || heartImages.Length == 0) return;

        //全てのアイコンを現在の体力と比較する
        for (int i = 0; i < heartImages.Length; i++)
        {
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

    //ランキングの更新

    /// <summary>
    /// タイトル画面のランキングUIを更新する
    /// </summary>
    private void UpdateTitleRankingUI()
    {
        if (RankingManager.Instance == null || titleRankingContent == null || rankingTextPrefab == null) return;

        // 既存のリストをクリア
        foreach (Transform child in titleRankingContent)
        {
            Destroy(child.gameObject);
        }

        var highScores = RankingManager.Instance.CurrentRanking.highScores;

        //スコアが一つもない場合の表示
        if (highScores.Count == 0)
        {
            GameObject obj = Instantiate(rankingTextPrefab, titleRankingContent);
            obj.GetComponent<TextMeshProUGUI>().text = "NO RECORD";
            return;
        }

        //スコアが高い順に生成して表示
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject obj = Instantiate(rankingTextPrefab, titleRankingContent);
            obj.GetComponent<TextMeshProUGUI>().text = $"{i + 1}. {highScores[i]}";
        }
    }

    /// <summary>
    /// ゲームオーバー画面のランキングを更新する
    /// </summary>
    private void UpdateGameOverRankingUI()
    {
        if (RankingManager.Instance == null || gameOverRankingTexts == null || gameOverRankingTexts.Length == 0) return;

        var highScores = RankingManager.Instance.CurrentRanking.highScores;

        //あらかじめ配置されたテキストを使いまわす
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

    /// <summary>
    /// 画面を暗くする演出
    /// </summary>
    /// <param name="duration">演出にかかる時間</param>
    public IEnumerator FadeOutRoutine(float duration)
    {
        if (fadePanelGroup == null) yield break;
        fadePanelGroup.gameObject.SetActive(true);
        fadePanelGroup.blocksRaycasts = true;

        //最前面に表示
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

    /// <summary>
    /// 画面を明るくする演出
    /// </summary>
    /// <param name="duration">演出にかかる時間</param>
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

    /// <summary>
    /// ゲーム開始前にプレイヤーが定位置に移動する演出
    /// </summary>
    /// <param name="playerTransform">プレイヤーのtransform</param>
    /// <param name="targetPosition">目標座標</param>
    public IEnumerator ReadyPresentationRoutine(Transform playerTransform, Vector2 targetPosition)
    {
        Vector2 startPosition = playerTransform != null ? (Vector2)playerTransform.position : targetPosition;
        startPosition.y = targetPosition.y;

        if (playerTransform != null)
        {

            //スクロール速度に合わせて移動させる
            float speed = GameManager.Instance.InitialScrollSpeed;
            float distance = Vector2.Distance(startPosition, targetPosition);
            float playerDuration = distance / speed;

            //プレイヤーの2倍の時間をかけてテキストのアニメーションも行う
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

    /// <summary>
    /// ゲーム開始前にテキストが横切る演出
    /// </summary>
    /// <param name="textDuration">演出の効果時間</param>
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
                //中央で少し遅くする
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

    /// <summary>
    /// Startテキストの演出が終わるのを待機するためのコルーチン
    /// </summary>
    public IEnumerator WaitTextExitRoutine()
    {
        while (startPresentationText != null && startPresentationText.gameObject.activeSelf)
        {
            yield return null;
        }
    }


   /// <summary>
   /// ゲーム内のHUDと操作パネルをフェードインさせる
   /// </summary>
   /// <param name="duration">演出にかかる時間</param>
    public IEnumerator FadeInGameplayUIRoutine(float duration)
    {
        if (hudPanelGroup != null) hudPanelGroup.alpha = 0.0f;
        if (touchInputPanelGroup != null) touchInputPanelGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (hudPanelGroup != null) hudPanelGroup.alpha = t;
            if (touchInputPanelGroup != null) touchInputPanelGroup.alpha = t;

            yield return null;
        }

        if (hudPanelGroup != null) hudPanelGroup.alpha = 1f;
        if (touchInputPanelGroup != null) touchInputPanelGroup.alpha = 1f;
    }

    /// <summary>
    /// 回避成功時のスコア加算テキストを表示する
    /// </summary>
    /// <param name="scoreValue">表示するスコア加算値</param>
    public void ShowBonusText(int scoreValue)
    {
        if (bonusTextPrefab == null || bonusTextSpawnPoint == null) return;

        GameObject obj = Instantiate(bonusTextPrefab, bonusTextSpawnPoint);

        BonusTextEffect effect = obj.GetComponent<BonusTextEffect>();
        if (effect != null)
        {
            effect.PlayEffect(scoreValue);
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

    public void OnClickOpenGuideButton()
    {
        if (guidePanel != null) guidePanel.SetActive(true);
    }

    public void OnClickCloseGuideButton()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
    }
}