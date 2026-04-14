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

        float elapsed = 0f;

        if (canvasGroup != null) canvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float easeOutT = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = Vector3.Lerp(startPos, endPos, easeOutT);

            if (canvasGroup != null)
            {
                if (t < 0.1f)
                {
                    canvasGroup.alpha = t / 0.1f;
                }
                else if (t > 0.7f)
                {
                    canvasGroup.alpha = 1f - ((t - 0.7f) / 0.3f);
                }
                else
                {
                    canvasGroup.alpha = 1f;
                }
            }

            yield return null;
        }
        Destroy(gameObject);
    }
}
