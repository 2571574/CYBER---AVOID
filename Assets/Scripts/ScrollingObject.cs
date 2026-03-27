using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    private GameStatus settings;

    public void SetSettings(GameStatus gs) => settings = gs;

    void Update()
    {
        if (settings == null) return;

        // 設定データに基づき左へ移動
        transform.Translate(Vector3.left * settings.scrollSpeed * Time.deltaTime);

        // 画面外（例：X=-15）に出たら削除してメモリを節約
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}