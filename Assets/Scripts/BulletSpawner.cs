using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 弾を生成するクラス
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("予告線用のプレハブ")]
    [SerializeField] private GameObject PredirectLinePrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [Header("Spawn Settings")]
    [SerializeField] private Transform bulletSpawnArea;

    private float bulletTimer;

    // プールの定義
    private ObjectPool<GameObject> bulletPool;
    private ObjectPool<GameObject> linePool;

    private void Awake()
    {
        bulletPool = CreatePool(bulletPrefab);
        linePool = CreatePool(PredirectLinePrefab);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            createFunc: () => {
                GameObject obj = Instantiate(prefab);
                // 生成時にPoolableObjectをアタッチし、自身のプールを覚えさせる
                PoolableObject poolable = obj.AddComponent<PoolableObject>();
                // Initializeは Awakeの段階ではまだ呼ばず、後で紐付けます
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false, // 本番環境向けにチェックを切って軽量化
            defaultCapacity: 20,
            maxSize: 100
        );
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
        if (state == GameState.Title || state == GameState.Playing)
        {
            bulletTimer = 0f;
        }
        if (state == GameState.Title || state == GameState.StartAnim)
        {
            StopAllCoroutines();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        bulletTimer += Time.deltaTime;
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedInterval = settings.baseBulletSpawnInterval / (1.0f + difficultyRise * settings.bulletIntervalWeight);
        float currentInterval = Mathf.Max(calculatedInterval, settings.minBulletSpawnInterval);

        if (bulletTimer >= currentInterval)
        {
            StartCoroutine(SpawnBulletRoutine(true));
            if (Random.value > 0.5f) StartCoroutine(SpawnBulletRoutine(false));
            bulletTimer = 0;
        }
    }

    private IEnumerator SpawnBulletRoutine(bool isPredictive)
    {
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.baseBulletFallSpeed * (1.0f + difficultyRise * settings.bulletSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxBulletFallSpeed);

        float calculatedWarningTime = settings.baseBulletWarningTime / (1.0f + difficultyRise * settings.bulletWarningTimeWeight);
        float currentWarningTime = Mathf.Max(calculatedWarningTime, settings.minBulletWarningTime);

        float ySpawn = bulletSpawnArea.position.y;
        float targetX = 0f;
        Vector2 screenRange = GameManager.Instance.GetDynamicScreenRange();

        if (isPredictive)
        {
            float yTarget = playerTransform.position.y;
            float t = (ySpawn - yTarget) / currentSpeed;
            float rawPredictedX = playerTransform.position.x + (playerRb.velocity.x * t);
            targetX = Mathf.Clamp(rawPredictedX, screenRange.x, screenRange.y);
        }
        else
        {
            targetX = Random.Range(screenRange.x, screenRange.y);
        }

        Vector3 spawnPos = new Vector3(targetX, ySpawn, 0);
        Vector3 targetPos = new Vector3(targetX, -10f, 0);

        // 予告線をプールから取得
        GameObject preLineObj = linePool.Get();
        preLineObj.GetComponent<PoolableObject>().Initialize(linePool);
        preLineObj.transform.position = spawnPos;

        LineRenderer lr = preLineObj.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, spawnPos);
            lr.SetPosition(1, targetPos);
        }

        PredictLineEffect warningEffect = preLineObj.GetComponent<PredictLineEffect>();
        if (warningEffect != null)
        {
            warningEffect.StartCharge(currentWarningTime);
        }

        yield return new WaitForSeconds(currentWarningTime);

        // 待機中にゲームオーバーやタイトルに戻っていた場合の安全対策
        if (preLineObj.activeSelf) preLineObj.GetComponent<PoolableObject>().ReleaseToPool();

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) yield break;

        // 弾をプールから取得
        GameObject bullet = bulletPool.Get();
        bullet.GetComponent<PoolableObject>().Initialize(bulletPool);
        bullet.transform.position = spawnPos;

        Rigidbody2D brb = bullet.GetComponent<Rigidbody2D>();
        if (brb) brb.velocity = Vector2.down * currentSpeed;

        // 3秒後にプールへ戻す
        bullet.GetComponent<PoolableObject>().ReleaseAfter(3.0f);
    }
}