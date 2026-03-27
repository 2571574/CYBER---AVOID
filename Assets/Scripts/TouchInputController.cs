using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInputController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float HorizontalInput { get; private set; } // -1: 左, 1: 右, 0: 入力なし
    public bool IsJumpHeld { get; private set; }       // ジャンプボタンを押し続けているか

    public event Action OnJumpStart;
    public event Action OnJumpEnd;

    [Tooltip("画面の下から何割の高さをジャンプエリアの境界線にするか（例: 0.15 なら下から15%より上がジャンプ）")]
    [SerializeField] private float jumpBorderHeightRatio = 0.15f;

    private float screenHalfWidth;
    private float jumpBorderY;

    private void Start()
    {
        screenHalfWidth = Screen.width / 2f;
        jumpBorderY = Screen.height * jumpBorderHeightRatio;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ProcessInput(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ProcessInput(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 指を離したらすべてリセット
        HorizontalInput = 0f;
        if (IsJumpHeld)
        {
            IsJumpHeld = false;
            OnJumpEnd?.Invoke();
        }
    }

    private void ProcessInput(Vector2 pointerPosition)
    {
        // 1. X座標で左右移動を判定
        HorizontalInput = pointerPosition.x < screenHalfWidth ? -1f : 1f;

        // 2. Y座標でジャンプ領域にいるかを判定
        bool isPointerInJumpArea = pointerPosition.y > jumpBorderY;

        // 状態が変化した瞬間のみイベントを発火
        if (isPointerInJumpArea && !IsJumpHeld)
        {
            IsJumpHeld = true;
            OnJumpStart?.Invoke();
        }
        else if (!isPointerInJumpArea && IsJumpHeld)
        {
            IsJumpHeld = false;
            OnJumpEnd?.Invoke();
        }
    }
}