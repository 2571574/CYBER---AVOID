using UnityEngine;

/// <summary>
/// この弾が一度回避の対象になったかどうかを管理するクラス
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("既にボーナススコアを獲得済みがどうかのフラグ")]
    public bool isDodged = false;

    private void OnEnable()
    {
        isDodged = false;
    }
}