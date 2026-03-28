using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [Header("ターゲットの画面比率 (幅, 高さ)")]
    public Vector2 targetAspect = new Vector2(9, 16);

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraRect();
    }

    void Update()
    {
        UpdateCameraRect();
    }

    void UpdateCameraRect()
    {
        if (cam == null) return;

        float targetRatio = targetAspect.x / targetAspect.y;
        float currentRatio = (float)Screen.width / Screen.height;

        float scaleHeight = currentRatio / targetRatio;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}