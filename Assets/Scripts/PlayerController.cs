using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [SerializeField] private TouchInputController inputController;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;     // プレイヤーの足元に配置する空オブジェクト
    [SerializeField] private float groundCheckRadius = 0.1f; // 判定の広さ
    [SerializeField] private LayerMask groundLayer;     // 地面として扱うレイヤー

    [Header("Rotation Settings")]
    [Tooltip("回転の速さ（X速度にかける倍率）")]
    [SerializeField] private float rotationSpeed = 30f;
    [Tooltip("着地時に姿勢を戻す滑らかさ")]
    [SerializeField] private float rotationLerpSpeed = 15f;
    [Tooltip("回転させる見た目のオブジェクト")]
    [SerializeField] private Transform visualTransform;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 startPosition;

    private float airRotationSpeed;
    private bool wasGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    private void Start()
    {
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

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.Playing)
        {
            transform.position = startPosition;
            rb.velocity = Vector2.zero;
        }
    }


    private void FixedUpdate()
    {
        // プレイ中以外は操作を受け付けない（GameManager制御）
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            rb.velocity = Vector2.zero; // 停止時は物理挙動も止める
            return;
        }

        //接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded && wasGrounded)
        {
            airRotationSpeed = -rb.velocity.x * rotationSpeed;
        }

        float targetVelocityX = inputController.HorizontalInput * settings.playerMoveSpeed;
        float currentVelocityX = rb.velocity.x;
        float currentAccel = isGrounded ? settings.groundAcceleration : settings.airAcceleration;
        float currentDecel = isGrounded ? settings.groundDeceleration : settings.airDeceleration;
        float accelRate = (Mathf.Abs(inputController.HorizontalInput) > 0.01f) ? currentAccel : currentDecel;
        float newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, accelRate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newVelocityX, rb.velocity.y);

        Vector2 screenRange = GameManager.Instance.GetDynamicScreenRange();
        float clampedX = Mathf.Clamp(rb.position.x, screenRange.x, screenRange.y);
        if (rb.position.x != clampedX)
        {
            rb.position = new Vector2(clampedX, rb.position.y);
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (inputController.IsJumpHeld && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, settings.jumpForce);
        }

        if (visualTransform != null)
        {
            if (!isGrounded)
            {
                visualTransform.Rotate(0, 0, airRotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                float currentAngle = visualTransform.eulerAngles.z;
                float targetAngle = Mathf.Round(currentAngle / 90f) * 90f;

                visualTransform.rotation = Quaternion.Lerp(visualTransform.rotation, Quaternion.Euler(0, 0, targetAngle), rotationLerpSpeed * Time.fixedDeltaTime);
            }
        }

        wasGrounded = isGrounded;
    }

    // 開発時にUnityエディタ上で判定範囲を視覚化するための機能
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}