using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField] private GameStatus settings;

    public float DifficultyMultiplier { get; private set; } = 1.0f;
    public float GlobalScrollMultiplier { get; private set; } = 1.0f;
    public float InitialScrollSpeed => settings != null ? settings.scrollSpeed : 8.0f;

    private float playTimer;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            playTimer += Time.deltaTime;
            if (settings != null)
            {
                DifficultyMultiplier = 1.0f + Mathf.Max(0f, (playTimer / settings.difficultyIncreaseInterval) * settings.difficultyIncreaseRate);
            }
        }
    }

    public void ResetStatus()
    {
        playTimer = 0f;
        DifficultyMultiplier = 1.0f;
        GlobalScrollMultiplier = 1.0f;
    }

    // GameManagerから呼ばれる、死亡時のスクロール停止演出用
    public void SetGlobalScrollMultiplier(float multiplier)
    {
        GlobalScrollMultiplier = multiplier;
    }
}
