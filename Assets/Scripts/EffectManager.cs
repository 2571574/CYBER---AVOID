using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{

    [Header("Effect Prefabs")]
    [Tooltip("被弾時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject damageEffectPrefab;
    [Tooltip("死亡時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject deathEffectPrefab;
    [Tooltip("ボーナス獲得時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject scoreEffectPrefab;

    private ObjectPool<GameObject> damagePool;
    private ObjectPool<GameObject> deathPool;
    private ObjectPool<GameObject> scorePool;
    private void Awake()
    {
        damagePool = CreatePool(damageEffectPrefab);
        deathPool = CreatePool(deathEffectPrefab);
        scorePool = CreatePool(scoreEffectPrefab);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () => {
                GameObject obj = Instantiate(prefab);
                obj.AddComponent<PoolableObject>();
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 30
        );
    }

    public void PlayEffect(ObjectPool<GameObject> pool, Vector3 position)
    {
        if (pool == null) return;

        GameObject effectObj = pool.Get();
        effectObj.transform.position = position;

        PoolableObject poolable = effectObj.GetComponent<PoolableObject>();
        poolable.Initialize(pool);

        ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            float duration = ps.main.duration + ps.main.startDelay.constantMax;

            poolable.ReleaseAfter(duration);
        }
        else
        {
            poolable.ReleaseAfter(3.0f);
        }

    }

    public void PlayDamageEffect(Vector3 position)
    {
        PlayEffect(damagePool, position);
    }
    public void PlayDeathEffect(Vector3 position)
    {
        PlayEffect(deathPool, position);
    }
    public void PlayScoreEffect(Vector3 position)
    {
        PlayEffect(scorePool, position);
    }
}