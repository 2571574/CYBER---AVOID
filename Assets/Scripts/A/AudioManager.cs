using System;
using UnityEngine;

/// <summary>
/// ゲーム全体の音を管理するクラス
/// </summary>
/// 
public enum BGMType
{
    Title,
    Play
}

public enum SEType
{
    Start,
    Death,
    Jump,
    Damage,
    Dodge,
    Alert,
    Button,
    UIOpen,
    UIClose
}

[Serializable]
public class BGMData
{
    public BGMType type;
    public AudioClip clip;
    [Tooltip("このBGMの個別音量")]
    [Range(0f, 1f)] public float volume = 1f;
}

[Serializable]
public class SEData
{
    public SEType type;
    public AudioClip clip;
    [Tooltip("このSEの個別音量")]
    [Range(0f, 1f)] public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Audio Clips")]
    [Tooltip("BGMのリストをここに登録します")]
    [SerializeField] private BGMData[] bgmDataList;
    [Tooltip("SEのリストをここに登録します")]
    [SerializeField] private SEData[] seDataList;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// BGMTypeを指定してBGMを再生する
    /// </summary>
    public void PlayBGM(BGMType type)
    {
        BGMData data = GetBGMData(type);
        if (data == null || data.clip == null) return;

        if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

        bgmSource.clip = data.clip;
        bgmSource.volume = data.volume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// SETypeを指定してSEを再生する
    /// </summary>
    public void PlaySE(SEType type)
    {
        SEData data = GetSEData(type);
        if (data == null || data.clip == null) return;

        seSource.PlayOneShot(data.clip, data.volume);
    }

    // Enumから対応するAudioClipを検索するヘルパーメソッド
    private BGMData GetBGMData(BGMType type)
    {
        foreach (var data in bgmDataList)
        {
            if (data.type == type) return data;
        }
        Debug.LogWarning($"BGMType {type} がAudioManagerに登録されていません。");
        return null;
    }

    private SEData GetSEData(SEType type)
    {
        foreach (var data in seDataList)
        {
            if (data.type == type) return data;
        }
        Debug.LogWarning($"SEType {type} がAudioManagerに登録されていません。");
        return null;
    }
}