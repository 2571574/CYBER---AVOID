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

    private void HandleStateChanged(GameState state) {
        if(state == GameState.Playing) {
            lineRenderer.enabled = true;
            TrailClear();
        }
        else {
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

    private void LateUpdate() {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
        if (settings == null) return;


        float difficultyRise = GameManager.Instance.DifficultyMultiplier - 1.0f;
        float calculatedSpeed = settings.scrollSpeed * (1.0f + difficultyRise * settings.ScrollSpeedWeight);
        float currentSpeed = Mathf.Min(calculatedSpeed, settings.maxScrollSpeed);
        float moveAmount = currentSpeed * Time.deltaTime;

        for (int i = 0; i < points.Count; i++) {
            points[i] = new Vector3(points[i].x - moveAmount, points[i].y, points[i].z);
        }

        points.Insert(0, transform.position);
        spawnTimes.Insert(0, Time.time);

        while(spawnTimes.Count > 0 && Time.time - spawnTimes[spawnTimes.Count - 1] > trailTime) {
            points.RemoveAt(points.Count - 1);
            spawnTimes.RemoveAt(spawnTimes.Count - 1);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
