using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [SerializeField] private TouchInputController inputController;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequest; // ジャンプ開始のフラグ
    private bool cutJumpRequest; // ジャンプを途中で切るフラグ

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        // イベントに登録
        inputController.OnJumpStart += HandleJumpStart;
        inputController.OnJumpEnd += HandleJumpEnd;
    }

    private void OnDisable()
    {
        // イベントから解除（メモリリーク防止）
        inputController.OnJumpStart -= HandleJumpStart;
        inputController.OnJumpEnd -= HandleJumpEnd;
    }

    private void HandleJumpStart()
    {
        jumpRequest = true; // FixedUpdateで処理する
    }

    private void HandleJumpEnd()
    {
        // ジャンプ中に指を離した場合
        if (!isGrounded && rb.velocity.y > 0)
        {
            cutJumpRequest = true;
        }
    }

    // 物理演算に関わる処理はFixedUpdateで行う
    private void FixedUpdate()
    {
        // 1. 左右移動の制御（アーケードライク：速度を直接上書き）
        float targetVelocityX = inputController.HorizontalInput * settings.playerMoveSpeed;
        rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);

        // 2. ジャンプの処理
        if (jumpRequest && isGrounded)
        {
            // 上向きの瞬時速度（力ではなく、速度を直接与える）
            rb.velocity = new Vector2(rb.velocity.x, settings.jumpForce);
            jumpRequest = false;
        }

        // 3. 可変ジャンプの処理（これが手触りの良さを生む）
        if (cutJumpRequest)
        {
            // 上向きの速度を半分にする（またはsettingsに追加しても良い）
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            cutJumpRequest = false;
        }
    }

    // 地面接地判定（簡易的な実装）
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}