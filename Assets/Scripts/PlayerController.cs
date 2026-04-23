using System;
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
    
    [Header("Effect Settings")]
    [Tooltip("接地時のトレイルのパーティクルシステム")]
    [SerializeField] private ParticleSystem groundSpark;
    [Tooltip("着地時のパーティクルシステム")]
    [SerializeField] private ParticleSystem groundImpact;

    private float soundCooldown = 0.05f;
    private float lastJumpSoundTime = 0f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 startPosition;

    private float airRotationSpeed;
    private bool wasGrounded;
    private float defaultGravity;

    public event Action OnJumped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        defaultGravity = rb.gravityScale;

        StopSparkEffect();
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
        if (state == GameState.StartAnim)
        {
            visualTransform.transform.rotation = Quaternion.identity;
            rb.velocity = Vector2.zero;
            StopSparkEffect();
        }

        if (state == GameState.Playing)
        {
            // ゲーム開始と同時に重力を元に戻す
            rb.gravityScale = defaultGravity;
        }
        else
        {
            // Title, Ready(フェードイン＆入場中), GameOver などは重力を完全に切る
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            if(state != GameState.CharaReady)
            {
                StopSparkEffect();
            }
        }
    }


    private void FixedUpdate()
    {
        // プレイ中以外は操作を受け付けない（GameManager制御）
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            rb.velocity = Vector2.zero; // 停止時は物理挙動も止める
            if (GameManager.Instance != null &&
                (GameManager.Instance.CurrentState == GameState.StartAnim ||
                GameManager.Instance.CurrentState == GameState.CharaReady))
            {
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
                if (groundSpark != null)
                {
                    var emission = groundSpark.emission;
                    emission.enabled = isGrounded;
                }

                wasGrounded = isGrounded;
            }
            else
            {
                StopSparkEffect();
            }
            return;
        }

        //接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (groundSpark != null)
        {
            var emission = groundSpark.emission;

            emission.enabled = isGrounded;

            if (isGrounded && !wasGrounded)
            {
                if (groundImpact != null)
                {
                    groundImpact.Play();
                }
            }

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

            Vector2 screenRange = CameraAspectController.Instance.GetDynamicScreenRange();
            float clampedX = Mathf.Clamp(rb.position.x, screenRange.x, screenRange.y);
            if (rb.position.x != clampedX)
            {
                rb.position = new Vector2(clampedX, rb.position.y);
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            if (inputController.IsJumpHeld && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, settings.jumpForce);

                if (Time.time - lastJumpSoundTime > soundCooldown)
                {
                    OnJumped?.Invoke();
                    lastJumpSoundTime = Time.time;
                }
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
    }

    private void StopSparkEffect()
    {
        if(groundSpark != null)
        {
            var emission  = groundSpark.emission;
            emission.enabled = false;
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