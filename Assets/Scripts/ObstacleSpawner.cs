using UnityEngine;
using UnityEngine.Pool;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject beamPrefab;
    [Header("Spawn Points")]
    [SerializeField] private Transform groundSpawnPoint;
    [SerializeField] private Transform airSpawnPoint;

    private float spawnTimer;

    private ObjectPool<GameObject> spikePool;
    private ObjectPool<GameObject> beamPool;

    private void Awake()
    {
        spikePool = CreatePool(spikePrefab);
        beamPool = CreatePool(beamPrefab);
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
            spawnTimer = 0f;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (settings == null) return;

        spawnTimer += Time.deltaTime;
        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedInterval = settings.baseObstacleSpawnInterval / (1.0f + difficultyRise * settings.obstacleIntervalWeight);
        float currentInterval = Mathf.Max(calculatedInterval, settings.minObstacleSpawnInterval);

        if (spawnTimer >= currentInterval)
        {
            SpawnRandomObstacle();
            spawnTimer = 0f;
        }
    }

    void SpawnRandomObstacle()
    {
        bool isSpike = Random.value > 0.5f;
        ObjectPool<GameObject> activePool = isSpike ? spikePool : beamPool;
        Vector3 spawnPos = isSpike ? groundSpawnPoint.position : airSpawnPoint.position;

        GameObject obj = activePool.Get();
        obj.GetComponent<PoolableObject>().Initialize(activePool);
        obj.transform.position = spawnPos;

        ScrollingObject scroller = obj.GetComponent<ScrollingObject>();
        if (scroller != null)
        {
            scroller.SetSettings(settings);
        }
    }
}