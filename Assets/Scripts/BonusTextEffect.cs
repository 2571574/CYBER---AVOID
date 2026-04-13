using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 回避ボーナスの文字エフェクト
/// </summary>
public class BonusTextEffect : MonoBehaviour
{
    [Tooltip("フェード用のCanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("テキストコンポーネント")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Animation Settings")]
    [Tooltip("アニメーションの全体時間")]
    [SerializeField] private float duration = 1.0f;
    [Tooltip("上に移動する距離")]
    [SerializeField] private float moveDistance = 50f;

    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    /// <param name="scoreValue">加算するスコア量</param>
    public void PlayEffect(int scoreValue)
    {
        if(text != null) text.text = "+" + scoreValue.ToString();
        StartCoroutine(AnimateRoutine());
    }

    /// <summary>
    /// 文字アニメーションの再生
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, moveDistance, 0);

        float halfDuration = duration / 2f;
        float elapsed = 0f;


        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            if (canvasGroup != null) canvasGroup.alpha = t;
            transform.localPosition = Vector3.Lerp(startPos, Vector3.Lerp(startPos, endPos, 0.5f), t);

            yield return null;
        }
        elapsed += Time.deltaTime;
        Vector3 midPos = transform.localPosition;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            transform.localPosition = Vector3.Lerp(midPos, endPos, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
