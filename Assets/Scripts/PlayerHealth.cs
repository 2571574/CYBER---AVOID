using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [Tooltip("点滅処理を行うためにプレイヤーのSpriteRendererを指定してください")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int CurrentHealth { get; private set; }
    private bool isInvincible = false;

    public bool IsInvincible => isInvincible;

    // HP変動時や死亡時にUI等へ知らせるイベント
    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDead;

    private void Start()
    {
        if (settings != null)
        {
            CurrentHealth = settings.maxHealth;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        if (settings != null)
        {
            CurrentHealth = settings.maxHealth;
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
        // タイトルに戻るたびにHPや状態を完全にリセットする
        if (state == GameState.Title || state == GameState.Playing)
        {
            CurrentHealth = settings.maxHealth;
            isInvincible = false;

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            gameObject.SetActive(true); // ゲームオーバーで非表示になっていた場合に戻す

            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isInvincible) return;

        // 障害物と弾のタグを別々に判定する（将来のボーナス判定のため）
        if (collision.CompareTag("Obstacle") || collision.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        CurrentHealth--;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnPlayerDead?.Invoke();
            GameManager.Instance.ChangeState(GameState.GameOver);

            // 暫定的な死亡処理：プレイヤーを非表示にする
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float blinkInterval = 0.1f;
        float timer = 0f;

        // 無敵時間中、SpriteRendererをオンオフして点滅させる
        while (timer < settings.invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // 点滅終了時は必ず表示状態に戻す
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
    }
}