using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [Tooltip("PlayerNeonShaderを適用したSpriteRendererを指定してください")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("HitEffect Settings")]
    [Tooltip("被弾時のフラッシュ色")]
    [SerializeField] private Color burstFlashColor = Color.red;
    [Tooltip("被弾時のフラッシュの強さ")]
    [SerializeField] private float burstMultiplier = 15.0f;

    [Header("Camera Shake Settings")]
    [Tooltip("被弾時の画面揺れの時間")]
    [SerializeField] private float damageShakeDuration = 0.15f;
    [Tooltip("被弾時の画面揺れの強さ")]
    [SerializeField] private float damageShakeMagnitude = 0.1f;
    [Tooltip("死亡時の画面揺れの時間")]
    [SerializeField] private float deathShakeDuration = 0.4f;
    [Tooltip("死亡時の画面揺れの強さ")]
    [SerializeField] private float deathShakeMagnitude = 0.25f;

    //現在の体力
    public int CurrentHealth { get; private set; }

    //シェーダーの縁の元の色を保存する変数
    private Color originalEdgeColor;

    //無敵状態
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;

    public event Action<int> OnHealthChanged;
    public event Action<Vector3, float, float> OnDamaged;
    public event Action<Vector3, float, float> OnPlayerDeadEvent;

    // シェーダーのプロパティIDをキャッシュ
    private readonly int damageRatioPropertyId = Shader.PropertyToID("_DamageRatio");
    private readonly int glitchIntensityPropertyId = Shader.PropertyToID("_GlitchIntensify");
    private readonly int edgeColorPropertyId = Shader.PropertyToID("_EdgeColor");

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
        // タイトル、スタートアニメーション、プレイ中の状態になったらHPをリセットして表示する
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

    /// <summary>
    /// 障害物や弾に衝突したときの処理
    /// </summary>
    /// <param name="collision">衝突したコライダ</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isInvincible) return;

        if (collision.CompareTag("Obstacle") || collision.CompareTag("Bullet"))
        {
            TakeDamage();
            if (collision.CompareTag("Bullet"))
            {
                //被弾時弾をプールに戻す
                var poolable = collision.GetComponent<PoolableObject>();
                if (poolable != null) poolable.ReleaseToPool();
            }
        }
    }

    /// <summary>
    /// ダメージを受けた時の処理
    /// </summary>
    private void TakeDamage()
    {
        //HPを減らす
        CurrentHealth--;
        OnHealthChanged?.Invoke(CurrentHealth);

        UpdateShaderDamageRatio();

        //被弾により死亡した場合
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            gameObject.SetActive(false);
            OnPlayerDeadEvent?.Invoke(transform.position, deathShakeDuration, deathShakeMagnitude);
        }
        //被弾時
        else
        {
            if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
            hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
            StartCoroutine(InvincibilityRoutine());
            OnDamaged?.Invoke(transform.position, damageShakeDuration, damageShakeMagnitude);
        }
    }

    /// <summary>
    /// 現在のHPに応じてシェーダーのダメージ比率を更新する
    /// </summary>
    private void UpdateShaderDamageRatio()
    {
        if (spriteRenderer != null && spriteRenderer.material != null && settings != null)
        {
            float ratio = 1f - ((float)CurrentHealth / settings.maxHealth);
            spriteRenderer.material.SetFloat(damageRatioPropertyId, ratio);
        }
    }

    /// <summary>
    /// ノイズ表現の強さを設定する。
    /// </summary>
    /// <param name="intensity">ノイズの強さ</param>
    private void SetGlitchIntensity(float intensity)
    {
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetFloat(glitchIntensityPropertyId, intensity);
        }
    }


    /// <summary>
    /// 被弾エフェクトのコルーチン
    /// </summary>
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

    /// <summary>
    /// 無敵時間のコルーチン
    /// </summary>
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