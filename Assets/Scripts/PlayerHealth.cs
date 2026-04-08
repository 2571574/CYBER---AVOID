using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [Tooltip("PlayerNeonShaderを適用したSpriteRendererを指定してください")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;

    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDead;

    // シェーダーのプロパティIDをキャッシュ
    private readonly int damageRatioPropertyId = Shader.PropertyToID("_DamageRatio");
    private readonly int glitchIntensityPropertyId = Shader.PropertyToID("_GlitchIntensify");

    private void Start()
    {
        if (settings != null) CurrentHealth = settings.maxHealth;
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;

        UpdateShaderDamageRatio();
        SetGlitchIntensity(0f); // 初期化時に歪みをゼロに
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.Playing)
        {
            if (settings != null) CurrentHealth = settings.maxHealth;
            isInvincible = false;

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            gameObject.SetActive(true);

            UpdateShaderDamageRatio();
            SetGlitchIntensity(0f);
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isInvincible) return;

        if (collision.CompareTag("Obstacle") || collision.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        CurrentHealth--;
        OnHealthChanged?.Invoke(CurrentHealth);

        // 穴の空き具合を更新
        UpdateShaderDamageRatio();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnPlayerDead?.Invoke();
            GameManager.Instance.ChangeState(GameState.GameOver);
            gameObject.SetActive(false);
        }
        else
        {
            // ヒットエフェクト（歪みの減衰）と無敵点滅を並行して実行
            StartCoroutine(HitGlitchRoutine());
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private void UpdateShaderDamageRatio()
    {
        if (spriteRenderer != null && spriteRenderer.material != null && settings != null)
        {
            float ratio = 1f - ((float)CurrentHealth / settings.maxHealth);
            spriteRenderer.material.SetFloat(damageRatioPropertyId, ratio);
        }
    }

    private void SetGlitchIntensity(float intensity)
    {
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetFloat(glitchIntensityPropertyId, intensity);
        }
    }

    // ダメージを受けた瞬間に激しく歪み、徐々に静止するヒットエフェクト
    private IEnumerator HitGlitchRoutine()
    {
        float duration = 0.3f; // 歪みが収まるまでの時間（秒）
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 1.0 から 0.0 へ向かって滑らかに減衰（SmoothStep的な補間）
            float t = elapsed / duration;
            float currentIntensity = Mathf.Lerp(1f, 0f, t * t); // 2乗することで後半スッと収まる

            SetGlitchIntensity(currentIntensity);
            yield return null;
        }

        SetGlitchIntensity(0f); // 最後に確実にゼロにする
    }

    private IEnumerator InvincibilityRoutine()
    {
        // ダメージを受けた瞬間から当たり判定は無敵にする
        isInvincible = true;

        // 無敵状態の視覚表現：全体を暗くし(RGB:0.3)、半透明(Alpha:0.5)にする
        if (spriteRenderer != null)
        {
            // Color(Red, Green, Blue, Alpha)
            spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
        }

        // 無敵時間全体を待機する
        yield return new WaitForSeconds(settings.invincibilityDuration);

        // 無敵時間終了：元の「真っ白で不透明」な状態に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isInvincible = false;
    }
}