using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class PlayerTrail : MonoBehaviour
{
    [SerializeField] private GameStatus settings;
    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.5f;
    [SerializeField] private Gradient trailGradient;


    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private List<float> spawnTimes = new List<float>();

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;

        lineRenderer.colorGradient = trailGradient;
    }

    private void Start() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy() {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.StartAnim || state == GameState.CharaReady || state == GameState.Playing)
        {
            lineRenderer.enabled = true;
            if (state == GameState.StartAnim) TrailClear(); // 演出開始時に一度クリア
        }
        else
        {
            // Title や GameOver になった時のみ消去する
            lineRenderer.enabled = false;
            TrailClear();
        }
    }

    private void TrailClear()
    {
        points.Clear();
        spawnTimes.Clear();
        lineRenderer.positionCount = 0;
    }

    private void LateUpdate()
    {
        if (GameManager.Instance == null) return;
        GameState currentState = GameManager.Instance.CurrentState;

        // ReadyとPlayingの時のみ処理を許可
        if (currentState != GameState.StartAnim && currentState != GameState.CharaReady && currentState != GameState.Playing) return;
        if (settings == null) return;

        float currentSpeed = 0f;

        // 背景や障害物による「後ろへの押し流し（スクロール）」はPlaying中のみ適用する
        if (currentState == GameState.Playing || currentState == GameState.CharaReady)
        {
            float difficultyRise = GameManager.Instance.Level.DifficultyMultiplier - 1.0f;
            float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
            currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed) * GameManager.Instance.Level.GlobalScrollMultiplier;
        }

        float moveAmount = currentSpeed * Time.deltaTime;

        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(points[i].x - moveAmount, points[i].y, points[i].z);
        }

        points.Insert(0, transform.position);
        spawnTimes.Insert(0, Time.time);

        while (spawnTimes.Count > 0 && Time.time - spawnTimes[spawnTimes.Count - 1] > trailTime)
        {
            points.RemoveAt(points.Count - 1);
            spawnTimes.RemoveAt(spawnTimes.Count - 1);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
