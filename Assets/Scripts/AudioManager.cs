using UnityEngine;

/// <summary>
/// ゲーム全体の音を管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("BGMを流すためのAudioSource")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("SEを流すためのAudioSource")]
    [SerializeField] private AudioSource seSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 指定のAudioClipのBGMを再生する
    /// </summary>
    /// <param name="clip">再生したいBGMのAudioClip</param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        //既に同じBGMが流れていたら流し続ける
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    /// <summary>
    /// 再生中のBGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 指定のAudioClipのSEを再生する
    /// </summary>
    /// <param name="clip">再生したいSEのAudioClip</param>
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip);
    }
}