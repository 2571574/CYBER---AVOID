using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DetectDodge : MonoBehaviour
{
    private PlayerHealth playerHealth;
    public event Action<Vector3> OnDodged;

    private void Start()
    {
        playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (collision.CompareTag("Bullet"))
        {
            // 弾のスクリプトを取得
            Bullet bullet = collision.GetComponent<Bullet>();

            // まだボーナスをもらっていない弾なら
            if (bullet != null && !bullet.isDodged)
            {
                if (playerHealth != null && !playerHealth.IsInvincible)
                {
                    // フラグを立てて二重加算を防ぐ
                    bullet.isDodged = true;

                    OnDodged?.Invoke(playerHealth.transform.position);
                }
            }
        }
    }
}