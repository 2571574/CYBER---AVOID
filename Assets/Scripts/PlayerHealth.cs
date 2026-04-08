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
    private readonly int edgeColorPropertyId = Shader.PropertyToID("_EdgeColor");

    private Color originalEdgeColor;
    private Coroutine hitEffectCoroutine;

    private void Start()
    {
        if (settings != null) CurrentHealth = settings.maxHealth;
        if (GameManager.Instance != null) GameManager.Instance.OnStateChanged += HandleStateChanged;

        // 初期化時にマテリアルの本来のネオンカラー（HDR）を保存
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
        if (state == GameState.Title || state == GameState.Playing)
        {
            if (settings != null) CurrentHealth = settings.maxHealth;
            isInvincible = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = Color.white; // 透過や色を完全にリセット

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
            OnPlayerDead?.Invoke();
            GameManager.Instance.ChangeState(GameState.GameOver);
            gameObject.SetActive(false);
        }
        else
        {
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

    // 発光バーストとグリッチを統合したヒットエフェクト
    private IEnumerator HitEffectRoutine()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        // HDR環境では4倍程度だと「少し白い」程度にしかならないため、極端に引き上げる（15倍〜20倍）
        float burstMultiplier = 15.0f;
        Color burstColor = originalEdgeColor * burstMultiplier;
        burstColor.a = originalEdgeColor.a; // アルファ値は元のまま維持

        // 1. バーストの最大発光を適用
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetColor(edgeColorPropertyId, burstColor);
        }

        // 2. 最大発光の状態を0.05秒間だけ「ホールド」する（これがないと一瞬すぎて見えない）
        yield return new WaitForSeconds(0.05f);

        // 3. 残りの時間で元の色へ減衰
        float fadeDuration = duration - 0.05f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // グリッチの減衰（2乗で後半にかけてスッと収束）
            float currentIntensity = Mathf.Lerp(1f, 0f, t * t);
            SetGlitchIntensity(currentIntensity);

            // 発光バーストの減衰
            // EaseOut（最初は早く暗くなり、後からゆっくり元に戻る）をかけて余韻を残す
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            if (spriteRenderer != null && spriteRenderer.material != null)
            {
                Color lerpedColor = Color.Lerp(burstColor, originalEdgeColor, easeT);
                spriteRenderer.material.SetColor(edgeColorPropertyId, lerpedColor);
            }

            yield return null;
        }

        // 終了時の確実なリセット
        SetGlitchIntensity(0f);
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetColor(edgeColorPropertyId, originalEdgeColor);
        }
    }

    // 無敵状態の制御（アルファ値のみを操作）
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // 発光バーストが最も強い瞬間（0.1秒間）は不透明度1.0を維持し、光を阻害しない
        yield return new WaitForSeconds(0.1f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        }

        // 残りの無敵時間を待機
        float remainingTime = Mathf.Max(0f, settings.invincibilityDuration - 0.1f);
        yield return new WaitForSeconds(remainingTime);

        // 無敵終了時に元の状態へ戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isInvincible = false;
    }
}