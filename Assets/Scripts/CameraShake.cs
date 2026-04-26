using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    //基準となる位置
    private Vector3 originalPosition;

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // カメラの初期位置を記憶しておく
        originalPosition = transform.position;
    }

    /// <summary>
    /// カメラを揺らす
    /// </summary>
    /// <param name="duration">持続時間</param>
    /// <param name="magnitude">強さ</param>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    /// <summary>
    /// カメラを揺らすコルーチン
    /// </summary>
    /// <param name="duration">持続時間</param>
    /// <param name="magnitude">強さ</param>
    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // ランダムな方向にカメラをずらす
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        shakeCoroutine = null;
    }
}