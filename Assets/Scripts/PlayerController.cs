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

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 startPosition;

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

        //ジャンプの処理
        if (inputController.IsJumpHeld && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, settings.jumpForce);
        }
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