using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class PredictLineEffect : MonoBehaviour
{
    private LineRenderer lr;

    [Header("Sound Settings")]
    [Tooltip("チャージ時間中に鳴らす回数")]
    [SerializeField] private int count = 3;

    [Header("Charge Settings")]
    [Tooltip("警告色（HDRで設定するとBloomで発光します）")]
    [SerializeField] private Color warningColor = Color.red;
    [Tooltip("チャージ開始時の最小の太さ")]
    [SerializeField] private float minWidth = 0.01f;
    [Tooltip("発射直前の最大の太さ")]
    [SerializeField] private float maxWidth = 0.15f;

    //チャージの基本時間
    private float chargeDuration = 1f;

    //現在のチャージ時間を管理する変数
    private float currentTimer = 0f;

    //チャージ中かどうかを管理するフラグ
    private bool isCharging = false;

    //チャージ中に音を鳴らした回数を管理する変数
    private int playedAlert = 0;

    public event Action OnAlertTriggered;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.numCapVertices = 4;
    }

    // BulletSpawnerから「発射までの時間」を受け取ってチャージを開始する
    public void StartCharge(float duration)
    {
        chargeDuration = duration;
        currentTimer = 0f;
        isCharging = true;
        playedAlert = 0;
        UpdateLine(0f); 
    }

    private void Update()
    {
        if (!isCharging) return;

        currentTimer += Time.deltaTime;

        //発射完了までの進捗度合い
        float progress = Mathf.Clamp01(currentTimer / chargeDuration);

        // 演出をリッチにするためのイージング
        float easeProgress = progress * progress;

        UpdateLine(easeProgress);

        if (count > 0 && playedAlert < count)
        {
            float nextAlert = (chargeDuration / count) * playedAlert;


            if (currentTimer > nextAlert)
            {
                OnAlertTriggered?.Invoke();
                playedAlert++;
            }

        }
        if (progress >= 1f)
        {
            isCharging = false;
        }
    }

    private void UpdateLine(float t)
    {
        // 太さのチャージ
        float currentWidth = Mathf.Lerp(maxWidth, minWidth, t);
        lr.startWidth = currentWidth;
        lr.endWidth = currentWidth;

        // 透明度（アルファ）のチャージ
        Color currentColor = warningColor;
        currentColor.a = Mathf.Lerp(0.0f, 1.0f, t);
        lr.startColor = currentColor;
        lr.endColor = currentColor;
    }
}
