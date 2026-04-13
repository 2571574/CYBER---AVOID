using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PoolableObject : MonoBehaviour
{
    private IObjectPool<GameObject> pool;

    public void Initialize(IObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    public void ReleaseToPool()
    {
        // 重複して解放しないようにアクティブ状態をチェック
        if (gameObject.activeSelf && pool != null)
        {
            pool.Release(gameObject);
        }
    }

    // 指定時間後に自動でプールに戻るコルーチン（弾の寿命用）
    public void ReleaseAfter(float time)
    {
        StartCoroutine(ReleaseRoutine(time));
    }

    private IEnumerator ReleaseRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        ReleaseToPool();
    }
}