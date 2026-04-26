using UnityEngine;

[CreateAssetMenu(fileName = "NewGameStatus", menuName = "CyberAvoid/GameStatus")]

/// <summary>
/// ゲーム全体の設定を管理する
/// </summary>
public class GameStatus : ScriptableObject
{
    [Header("Player Settings")]
    [Tooltip("プレイヤーの最大ライフ")]
    public int maxHealth = 3;
    [Tooltip("被弾時の無敵時間（秒）")]
    public float invincibilityDuration = 1.5f;
    [Tooltip("プレイヤーの左右移動速度")]
    public float playerMoveSpeed = 5.0f;
    [Tooltip("ジャンプ力")]
    public float jumpForce = 6.0f;
    [Tooltip("地上での加速度")]
    public float groundAcceleration = 20.0f;
    [Tooltip("地上での摩擦/減速度")]
    public float groundDeceleration = 20.0f;
    [Tooltip("空中での加速度")]
    public float airAcceleration = 10.0f;
    [Tooltip("空中での摩擦/減速度")]
    public float airDeceleration = 5.0f;

    [Header("Enemy / Bullet Settings")]
    [Tooltip("スクロール速度")]
    public float scrollSpeed = 8.0f;
    [Tooltip("予測射撃の弾の落下速度")]
    public float baseBulletFallSpeed = 15.0f;
    [Tooltip("障害物の生成間隔（秒）")]
    public float maxObstacleSpawnInterval = 3.0f;

    [Header("World Settings")]
    [Tooltip("難易度上昇倍率")]
    public float difficultyIncreaseRate = 0.1f;
    [Tooltip("難易度上昇までの時間")]
    public float difficultyIncreaseInterval = 10.0f;

    [Header("Difficulty Settings")]
    [Tooltip("難易度によるスクロール速度の上昇値")]
    public float ScrollSpeedWeight = 1.0f;
    [Tooltip("スクロールの最大速度")]
    public float maxScrollSpeed = 15.0f;

    public float baseObstacleSpawnInterval = 2.5f;
    [Tooltip("難易度による障害物生成頻度の上昇値")]
    public float obstacleIntervalWeight = 1.0f;
    [Tooltip("障害物の最短生成間隔")]
    public float minObstacleSpawnInterval = 1.0f;

    [Tooltip("難易度による弾の落下速度の上昇値")]
    public float bulletSpeedWeight = 0.0f;
    [Tooltip("弾の最大落下速度")]
    public float maxBulletFallSpeed = 25.0f;

    public float baseBulletWarningTime = 1.0f;
    [Tooltip("難易度による予告線表示時間の短縮値")]
    public float bulletWarningTimeWeight = 1.0f;
    [Tooltip("予告線の最短表示時間")]
    public float minBulletWarningTime = 0.4f;

    public float baseBulletSpawnInterval = 3.0f;
    [Tooltip("難易度による弾の生成頻度の上昇値")]
    public float bulletIntervalWeight = 1.0f;
    [Tooltip("弾の最短生成間隔")]
    public float minBulletSpawnInterval = 0.8f;

    [Header("Score Settings")]
    [Tooltip("1秒生存あたりの加算スコア")]
    public float scorePerSecond = 10f;
    [Tooltip("回避成功時のボーナススコア")]
    public float dodgeBonusScore = 50f;
}