using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    private GameStatus settings;

    /// <summary>
    /// スクロールの計算に必要な設定を受け取る
    /// </summary>
    /// <param name="gs"></param>
    public void SetSettings(GameStatus gs)
    {
        settings = gs;
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.PlayerDead) return;
        if (settings == null) return;

        //難易度によるスクロール速度を計算
        float difficultyRise = GameManager.Instance.Level.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);

        transform.Translate(Vector3.left * currentSpeed * GameManager.Instance.Level.GlobalScrollMultiplier * Time.deltaTime);

        //画面外に出たらオブジェクトを削除
        if (transform.position.x < -15f)
        {
            PoolableObject poolable = GetComponent<PoolableObject>();
            if (poolable != null) 
            {
                poolable.ReleaseToPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}