using UnityEngine;

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

    private void HandleStateChanged(GameState state)
    {
        // タイトルに戻った時やリトライ時に背景の位置をリセット
        if (state == GameState.Title || state == GameState.Playing)
        {
            transform.position = startPosition;
        }
    }

    private void Update()
    {
        // プレイ中のみ動作
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (settings == null) return;

        // 障害物と完全に同じ計算式でスピードを算出
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);

        // Vector3.left（右から左）へ移動
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        // 画像1枚分（spriteWidth）左に移動したら、右にワープさせて無限ループ
        if (transform.position.x <= startPosition.x - spriteWidth)
        {
            // 移動した分だけ右に位置を戻す
            transform.position += new Vector3(spriteWidth, 0, 0);
        }
    }
}