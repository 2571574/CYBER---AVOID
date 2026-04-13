using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DetectDodge : MonoBehaviour
{
    // 既にボーナスを獲得した弾のコライダーを記憶し、多重加算を防ぐ
    private HashSet<Collider2D> dodgedBullets = new HashSet<Collider2D>();
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void Update()
    {
        if (dodgedBullets.Count> 0)
        {
            dodgedBullets.RemoveWhere(col => col == null || !col.gameObject.activeInHierarchy);
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
        // タイトルやリトライで最初からになる時、記憶リストを綺麗に空にする（メモリ対策）
        if (state == GameState.Title || state == GameState.Playing)
        {
            dodgedBullets.Clear();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        // タグが「Bullet」であり、かつまだボーナスをもらっていない弾なら
        if (collision.CompareTag("Bullet"))
        {
            if (!dodgedBullets.Contains(collision))
            {
                if (playerHealth != null && !playerHealth.IsInvincible)
                {
                    // 弾をリストに登録し、ボーナスを加算
                    dodgedBullets.Add(collision);

                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddDodgeBonus();
                    }

                    if(EffectManager.Instance != null && playerHealth != null)
                    {
                        EffectManager.Instance.PlayScoreEffect(playerHealth.transform.position);
                    }
                }
            }
        }
    }
}