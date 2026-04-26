using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class TouchInputController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float HorizontalInput { get; private set; } // -1: 左, 1: 右, 0: 入力なし
    public bool IsJumpHeld { get; private set; }       // ジャンプ判定

    [Header("Movement Settings (X Axis)")]
    [Tooltip("パネルの左端から何割の幅を『左移動エリア』にするか")]
    [SerializeField] private float leftMoveAreaRatio = 0.35f;

    [Tooltip("パネルの右端から何割の幅を『右移動エリア』にするか")]
    [SerializeField] private float rightMoveAreaRatio = 0.35f;

    [Header("Jump Settings (Y Axis)")]
    [Tooltip("パネルの上から何割を『ジャンプエリア』にするか（例: 0.5なら上半分がジャンプ）")]
    [SerializeField] private float jumpAreaRatioInPanel = 0.5f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData) => ProcessInput(eventData);

    public void OnDrag(PointerEventData eventData) => ProcessInput(eventData);

    public void OnPointerUp(PointerEventData eventData) => ResetInput();

    private void OnDisable()
    {
        ResetInput();
    }

    private void ResetInput()
    {
        HorizontalInput = 0f;
        IsJumpHeld = false;
    }

    private void ProcessInput(PointerEventData eventData)
    {
        // タップされたスクリーン座標を、このUIパネル内のローカル座標に変換
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            Rect rect = rectTransform.rect;

            // XとYを、パネルの左下を 0.0、右上を 1.0 とした割合に変換
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;

            // X軸の判定
            if (normalizedX < leftMoveAreaRatio)
            {
                HorizontalInput = -1f;
            }
            else if (normalizedX > (1.0f - rightMoveAreaRatio))
            {
                HorizontalInput = 1f;
            }
            else
            {
                HorizontalInput = 0f; // 中央エリア
            }

            // Y軸の判定
            IsJumpHeld = normalizedY > (1.0f - jumpAreaRatioInPanel);
        }
    }
}