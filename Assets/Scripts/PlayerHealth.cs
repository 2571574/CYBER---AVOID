using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [Tooltip("PlayerNeonShaderを適用したSpriteRendererを指定してください")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("HitEffect Settings")]
    [SerializeField] private Color burstFlashColor = Color.red;
    [SerializeField] private float burstMultiplier = 15.0f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float damageShakeDuration = 0.15f;
    [SerializeField] private float damageShakeMagnitude = 0.1f;
    [SerializeField] private float deathShakeDuration = 0.4f;
    [SerializeField] private float deathShakeMagnitude = 0.25f;

    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;

    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDead;

    // シェーダーのプロパティIDをキャッシュ
    private readonly int damageRatioPropertyId = Shader.PropertyToID("_DamageRatio");
    private readonly int glitchIntensityPropertyId = Shader.PropertyToID("_GlitchIntensify");
    private readonly int edgeColorPropertyId = Shader.PropertyToID("_EdgeColor");

    private Color originalEdgeColor;
    private Coroutine hitEffectCoroutine;

    private void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;

        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            originalEdgeColor = spriteRenderer.material.GetColor(edgeColorPropertyId);
        }

        UpdateShaderDamageRatio();
        SetGlitchIntensity(0f);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.StartAnim || state == GameState.Playing)
        {
            if (settings != null) CurrentHealth = settings.maxHealth;
            isInvincible = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = Color.white;

                if (spriteRenderer.material != null)
                {
                    spriteRenderer.material.SetColor(edgeColorPropertyId, originalEdgeColor);
                }
            }
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
            if (collision.CompareTag("Bullet"))
            {
                var poolable = collision.GetComponent<PoolableObject>();
                if (poolable != null) poolable.ReleaseToPool();
            }
        }
    }

    private void TakeDamage()
    {
        CurrentHealth--;
        OnHealthChanged?.Invoke(CurrentHealth);

        UpdateShaderDamageRatio();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if(EffectManager.Instance != null)
                EffectManager.Instance.PlayDeathEffect(transform.position);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(deathShakeDuration, deathShakeMagnitude);
            OnPlayerDead?.Invoke();
            gameObject.SetActive(false);
            GameManager.Instance.HandlePlayerDeath();
        }
        else
        {
            if (AudioManager.Instance != null) 
            {
                AudioManager.Instance.PlaySE(SEType.Damage);
            }
            if (EffectManager.Instance != null)
                EffectManager.Instance.PlayDamageEffect(transform.position);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(damageShakeDuration, damageShakeMagnitude);
            // 既存のエフェクトが実行中なら停止して上書き
            if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);

            hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
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


    private IEnumerator HitEffectRoutine()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Color burstColor = burstFlashColor * burstMultiplier;
        burstColor.a = originalEdgeColor.a;

        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetColor(edgeColorPropertyId, burstColor);
        }

       
        yield return new WaitForSeconds(0.05f);

        float fadeDuration = duration - 0.05f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            float currentIntensity = Mathf.Lerp(1f, 0f, t * t);
            SetGlitchIntensity(currentIntensity);

            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            if (spriteRenderer != null && spriteRenderer.material != null)
            {
                Color lerpedColor = Color.Lerp(burstColor, originalEdgeColor, easeT);
                spriteRenderer.material.SetColor(edgeColorPropertyId, lerpedColor);
            }

            yield return null;
        }

        SetGlitchIntensity(0f);
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetColor(edgeColorPropertyId, originalEdgeColor);
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        }

        float remainingTime = Mathf.Max(0f, settings.invincibilityDuration - 0.1f);
        yield return new WaitForSeconds(remainingTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isInvincible = false;
    }
}