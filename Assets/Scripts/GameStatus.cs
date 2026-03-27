using UnityEngine;

[CreateAssetMenu(fileName = "NewGameStatus", menuName = "CyberAvoid/GameStatus")]
public class GameStatus : ScriptableObject
{
    [Header("Player Settings")]
    [Tooltip("プレイヤーの左右移動速度")]
    public float playerMoveSpeed = 5.0f;
    [Tooltip("ジャンプ力")]
    public float jumpForce = 12.0f;
    [Tooltip("落下時の重力倍率（操作性を重くするため）")]
    public float fallGravityMultiplier = 2.5f;

    [Header("Input Settings")]
    [Tooltip("ジャンプと判定するY軸方向のスワイプ量")]
    public float swipeJumpThreshold = 50.0f;

    [Header("World Settings")]
    [Tooltip("障害物が迫ってくるスクロール速度")]
    public float scrollSpeed = 8.0f;

    [Header("Enemy / Bullet Settings")]
    [Tooltip("予測射撃の弾の落下速度")]
    public float bulletFallSpeed = 15.0f;
}
