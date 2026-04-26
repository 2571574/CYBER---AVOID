using TMPro;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム内全てのUIを管理するクラス
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("タイトル画面のパネル")]
    [SerializeField] private GameObject titlePanel;
    [Tooltip("ゲームオーバー画面のパネル")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("ランキング画面のパネル")]
    [SerializeField] private GameObject rankingPanel;
    [Tooltip("ガイド画面のパネル")]
    [SerializeField] private GameObject guidePanel;
    [Tooltip("クレジット画面のパネル")]
    [SerializeField] private GameObject creditsPanel;

    [Tooltip("プレイ中HUDのパネル")]
    [SerializeField] private GameObject hudPanel;
    [Tooltip("フェードインのためのHUDのCanvasGroup")]
    [SerializeField] private CanvasGroup hudPanelGroup;
    [Tooltip("操作パネル")]
    [SerializeField] private GameObject touchInputPanel;
    [Tooltip("フェードインのための操作パネルのCanvasGroup")]
    [SerializeField] private CanvasGroup touchInputPanelGroup;

    [Header("HUD Elements")]
    [Tooltip("名前入力用のインプットフィールド")]
    [SerializeField] private TMP_InputField nameInputField;
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
    [Header("Result Elements")]
    [Tooltip("ゲームオーバー画面で今回のスコアを表示するテキスト")]
    [SerializeField] private TextMeshProUGUI resultScoreText;

    [Header("Ranking Elements")]
    [Tooltip("タイトルでランキングを一覧で並べるオブジェクト")]
    [SerializeField] private Transform titleRankingContent;
    [Tooltip("ゲームオーバーでランキングを一覧で並べるオブジェクト")]
    [SerializeField] private Transform gameOverRankingContent;
    [Tooltip("ランキング表示エリアの透明度を管理するCanvasGroup")]
    [SerializeField] private CanvasGroup rankingCanvasGroup;
    [Tooltip("1順位ずつ表示させるためのテキストのプレハブ")]
    [SerializeField] private GameObject rankingTextPrefab;

    [Header("Transition Elements")]
    [Tooltip("フェード用の黒いパネルのCanvasGroup")]
    [SerializeField] private CanvasGroup fadePanelGroup;
    [Tooltip("ゲーム開始時のStartのテキスト")]
    [SerializeField] private TextMeshProUGUI startPresentationText;

    [Header("References")]
    [Tooltip("PlayerHealthの参照")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Sound Setting")]
    [Tooltip("ボタンを押した時のSE")]
    [SerializeField] private AudioClip buttonSE;

    public event Action<string> OnStartRequested;
    public event Action<string> OnRetryRequested;
    public event Action OnTitleRequested;
    public event Action OnRankingOpenRequested;
    public event Action<SEType> OnUIActionSoundRequested;
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

        if (GameManager.Instance != null && GameManager.Instance.Score != null)
        {
            GameManager.Instance.Score.OnScoreUpdated += UpdateScoreText;
        }
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void UpdateScoreText(float newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + Mathf.FloorToInt(newScore).ToString();
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

        if (GameManager.Instance != null && GameManager.Instance.Score != null)
        {
            GameManager.Instance.Score.OnScoreUpdated -= UpdateScoreText;
        }
    }

    public void UpdateRankingDisplay(bool isGameOverSequence, bool isOffline)
    {
        if (isGameOverSequence)
        {
            UpdateGameOverRankingUI(isOffline);
        }
        else
        {
            UpdateTitleRankingUI(isOffline);
        }
    }

    public void ShowRankingLoading(bool isGameOverSequence)
    {
        if (isGameOverSequence)
        {
            ShowMessageInRankingUI(gameOverRankingContent, "LOADING...");
        }
        else
        {
            ShowMessageInRankingUI(titleRankingContent, "LOADING...");
        }
    }
    public void SetResultScoreText(int finalScore)
    {
        if (resultScoreText != null)
        {
            resultScoreText.text = "SCORE: " + finalScore.ToString();
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
    private void GenerateRankingUI(Transform targetContent, bool isOffline = false)
    {
        if (targetContent == null || rankingTextPrefab == null) return;

        // 既存のリストをクリア
        foreach (Transform child in targetContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        if (isOffline)
        {
            GameObject obj = Instantiate(rankingTextPrefab, targetContent);
            obj.GetComponent<TextMeshProUGUI>().text = "OFFLINE";
            return;
        }

        var rankData = GameManager.Instance.Ranking.CurrentRanking;

        if (rankData.Count == 0)
        {
            GameObject obj = Instantiate(rankingTextPrefab, targetContent);
            obj.GetComponent<TextMeshProUGUI>().text = "NO RECORD";
            return;
        }

        for (int i = 0; i < rankData.Count; i++)
        {
            GameObject obj = Instantiate(rankingTextPrefab, targetContent);
            string rankNum = (i < 9) ? $"<color=#00000000>0</color>{i + 1}" : $"{i + 1}";
            obj.GetComponent<TextMeshProUGUI>().text = $"{rankNum}.<space=0.5em>{rankData[i].PlayerName} <pos=375>:<space=0.5em>{rankData[i].Score}";
        }
    }
    /// <summary>
    /// タイトル画面のランキングUIを更新する
    /// </summary>
    private void UpdateTitleRankingUI(bool isOffline = false)
    {
        GenerateRankingUI(titleRankingContent, isOffline);
    }

    private void UpdateGameOverRankingUI(bool isOffline = false)
    {
        GenerateRankingUI(gameOverRankingContent, isOffline);
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
            float speed = GameManager.Instance.Level.InitialScrollSpeed;
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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Audio.PlaySE(SEType.Start);
            }
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

    /// <summary>
    /// ランキングUIに任意のメッセージ（LOADING...など）を1行だけ表示し、古いリストをクリアする
    /// </summary>
    private void ShowMessageInRankingUI(Transform targetContent, string message)
    {
        if (targetContent == null || rankingTextPrefab == null) return;

        // 既存のリストをクリア
        foreach (Transform child in targetContent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // メッセージを表示
        GameObject obj = Instantiate(rankingTextPrefab, targetContent);
        obj.GetComponent<TextMeshProUGUI>().text = message;
    }
    // --- ボタンメソッド ---
    public void OnClickStartButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.Button);
        string playerName = nameInputField != null ? nameInputField.text : "";
        OnStartRequested?.Invoke(playerName);
    }

    public void OnClickTitleButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.Button);
        OnTitleRequested?.Invoke();
    }

    public void OnClickRetryButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.Button);
        string playerName = nameInputField != null ? nameInputField.text : "";
        OnRetryRequested?.Invoke(playerName);
    }

    public void OnClickOpenRankingButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIOpen);
        if (rankingPanel != null) rankingPanel.SetActive(true);
        OnRankingOpenRequested?.Invoke();
    }

    public void OnClickCloseRankingButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIClose);
        if (rankingPanel != null) rankingPanel.SetActive(false);
    }

    public void OnClickOpenGuideButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIOpen);
        if (guidePanel != null) guidePanel.SetActive(true);
    }

    public void OnClickCloseGuideButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIClose);
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    public void OnClickOpenCreditsButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIOpen);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void OnClickCloseCreditsButton()
    {
        OnUIActionSoundRequested?.Invoke(SEType.UIClose);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
}