using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    private GameStatus settings;

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

        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;

        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);

        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

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