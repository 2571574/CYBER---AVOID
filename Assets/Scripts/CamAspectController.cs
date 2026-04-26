using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]

/// <summary>
/// カメラのアスペクト比を制御するクラス
/// </summary>
public class CameraAspectController : MonoBehaviour
{
    public static CameraAspectController Instance { get; private set; }
    [Header("ターゲットの画面比率 (幅, 高さ)")]
    public Vector2 targetAspect = new Vector2(9, 16);

    private Camera cam;

    //以前の画面サイズを記憶する変数
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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

    /// <summary>
    /// 現在の画面サイズとターゲットのアスペクト比からカメラを調整する
    /// </summary>
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

    /// <summary>
    /// 現在のカメラの描画範囲に基づき、ワールド座標での左右の範囲を取得
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDynamicScreenRange()
    {
        if (cam != null)
        {
            float halfWidth = cam.orthographicSize * cam.aspect;
            return new Vector2(-halfWidth + 0.15f, halfWidth - 0.15f);
        }
        return new Vector2(-2.8f, 2.8f);
    }
}