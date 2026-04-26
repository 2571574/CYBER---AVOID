using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] private GameStatus settings;

    //現在のスコア
    public float CurrentScore { get; private set; }

    public event Action<float> OnScoreUpdated;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Title || state == GameState.Playing)
        {
            CurrentScore = 0f;
        }
    }

    private void Update()
    {
        // プレイ中のみ、時間経過でスコアを加算
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (settings != null)
        {
            CurrentScore += settings.scorePerSecond * Time.deltaTime;
            OnScoreUpdated?.Invoke(CurrentScore);
        }
    }

    /// <summary>
    /// 回避成功時のスコア加算
    /// </summary>
    public int AddDodgeBonus()
    {
        if (settings != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            CurrentScore += settings.dodgeBonusScore;
            OnScoreUpdated?.Invoke(CurrentScore);
            return Mathf.FloorToInt(settings.dodgeBonusScore);
        }
        return 0;
    }
}