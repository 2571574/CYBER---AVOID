using UnityEngine;

/// <summary>
/// 背景のスクロールを行うクラス
/// </summary>
public class BackgroundScroller2D : MonoBehaviour
{
    [SerializeField] private GameStatus settings;

    [Tooltip("背景画像1枚分の横幅（Unityの座標距離）を入力してください")]
    [SerializeField] private float spriteWidth;

    private Vector3 startPosition;

    private void Start()
    {
        // 初期位置を記憶
        startPosition = transform.position;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    /// <summary>
    /// ステートが変わった時に行う処理
    /// </summary>
    /// <param name="state"></param>
    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.StartAnim)
        {
            transform.position = startPosition;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.CharaReady &&
            GameManager.Instance.CurrentState != GameState.PlayerDead) return;
        if (settings == null) return;

        // 障害物のスピードと同じ計算式
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);

        // 左へスクロール
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        // 画像1枚分スクロールしたことを検知
        if (transform.position.x <= startPosition.x - spriteWidth)
        {
            // 移動した分だけ位置を戻す
            transform.position += new Vector3(spriteWidth, 0, 0);
        }
    }
}