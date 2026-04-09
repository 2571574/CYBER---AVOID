using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    [Tooltip("被弾時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject damageEffectPrefab;
    [Tooltip("死亡時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject deathEffectPrefab;
    [Tooltip("ボーナス獲得時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject scoreEffectPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        Instantiate(prefab, position, Quaternion.identity);
    }

    public void PlayDamageEffect(Vector3 position)
    {
        PlayEffect(damageEffectPrefab, position);
    }

    // 死亡時用のショートカット関数を追加
    public void PlayDeathEffect(Vector3 position)
    {
        PlayEffect(deathEffectPrefab, position);
    }

    public void PlayScoreEffect(Vector3 position)
    {
        PlayEffect(scoreEffectPrefab, position);
    }
}