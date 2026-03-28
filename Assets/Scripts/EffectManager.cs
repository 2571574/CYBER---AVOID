using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    [Tooltip("被弾時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject damageEffectPrefab;
    [Tooltip("ボーナス獲得時に再生するパーティクルプレハブを紐付けてください")]
    [SerializeField] private GameObject scoreEffectPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // === エフェクトの再生処理 ===
    // 指定された位置にプレハブを生成（Instantiate）します
    public void PlayEffect(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        // 生成と同時に、Quaternion.identity（回転なし）で配置
        Instantiate(prefab, position, Quaternion.identity);
    }

    // === 外部から呼び出すためのショートカット関数 ===

    // プレイヤーが被弾した場所で呼び出します
    public void PlayDamageEffect(Vector3 position)
    {
        PlayEffect(damageEffectPrefab, position);
    }

    // ボーナスを獲得した場所で呼び出します
    public void PlayScoreEffect(Vector3 position)
    {
        PlayEffect(scoreEffectPrefab, position);
    }
}