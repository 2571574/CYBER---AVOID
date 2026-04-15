using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class CameraAspectController : MonoBehaviour
{
    [Header("ターゲットの画面比率 (幅, 高さ)")]
    public Vector2 targetAspect = new Vector2(9, 16);

    private Camera cam;

    private int lastScreenWidth;
    private int lastScreenHeight;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 起動時の画面サイズを記憶
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        UpdateCameraRect();
    }

    void Update()
    {
        // 画面の幅か高さが、前回記憶した値と異なる場合のみ計算処理を実行
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraRect();

            // 新しい画面サイズを記憶し直す
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
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