using UnityEngine;

/// <summary>
/// 背景のスクロールを行うクラス
/// </summary>
public class BackgroundScroller2D : MonoBehaviour
{
    [Tooltip("ゲーム内のパラメータ")]
    [SerializeField] private GameStatus settings;

    private float spriteWidth;      //背景の画像１枚分の横幅
    private Vector3 startPosition;  //リセットするための初期位置

    private void Start()
    {
        startPosition = transform.position;
        
        //背景のオブジェクトから画像1枚の横幅を取得
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if(sr != null){
            spriteWidth = sr.bounds.size.x;
        }

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
    /// <param name="state">変更後のステート</param>
    private void HandleStateChanged(GameState state)
    {
        //タイトルかスタート直前に背景を初期位置に戻す
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

        //難易度によるスクロール速度を計算
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);

        // スクロール
        transform.Translate(Vector3.left * currentSpeed * GameManager.Instance.GlobalScrollMultiplier * Time.deltaTime);

        // ループ処理
        if (transform.position.x <= startPosition.x - spriteWidth)
        {
            transform.position += new Vector3(spriteWidth, 0, 0);
        }
    }
}