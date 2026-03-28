using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GridScroll : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.5f, 0f);
    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (rawImage != null)
        {
            // 現在の uvRect を取得
            Rect uvRect = rawImage.uvRect;

            // 位置（オフセット）をスクロール速度に応じて加算
            uvRect.position += scrollSpeed * Time.deltaTime;

            // 更新した uvRect を戻す
            rawImage.uvRect = uvRect;
        }
    }
}
