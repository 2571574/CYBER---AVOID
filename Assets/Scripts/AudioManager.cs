using UnityEngine;

/// <summary>
/// 音を管理する
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("BGMを流すためのAudioSourceを紐付けてください")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("効果音(SE)を流すためのAudioSourceを紐付けてください")]
    [SerializeField] private AudioSource seSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// BGMの再生、停止
    /// </summary>
    /// <param name="clip"></param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // すでに同じBGMが流れている場合は最初から再生し直さない
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// SEの再生
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        // PlayOneShotを使うことで、音が重なっても途切れずに複数鳴らすことができます
        seSource.PlayOneShot(clip);
    }
}