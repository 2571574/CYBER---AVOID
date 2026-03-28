using System;
using UnityEngine;

public enum GameState
{
    Title,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    private float playTimer;
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    public float DifficultyMultiplier { get; private set; } = 1.0f;
    [SerializeField] private GameStatus settings;

    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        ChangeState(GameState.Title);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            playTimer += Time.deltaTime;
            if (settings != null)
            {
                DifficultyMultiplier = 1.0f + Mathf.Max(0f, (playTimer / settings.difficultyIncreaseInterval) * settings.difficultyIncreaseRate);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        if (newState == GameState.Title)
        {
            playTimer = 0f;
            DifficultyMultiplier = 1.0f;
        }
        OnStateChanged?.Invoke(newState);
    }

    public void GoToTitle()
    {
        ResetStatus();
        ClearField();
        ChangeState(GameState.Title);
    }

    public void StartGame()
    {
        ResetStatus();
        ClearField();
        ChangeState(GameState.Playing);
    }

    public void RetryGame()
    {
        ResetStatus();
        ClearField();
        ChangeState(GameState.Playing);
    }

    private void ResetStatus()
    {
        playTimer = 0f;
        DifficultyMultiplier = 1.0f;
    }

    private void ClearField()
    {
    }
    public Vector2 GetDynamicScreenRange()
    {
        if (Camera.main != null)
        {
            float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            // キャラクターが画面外に半分はみ出ないよう、少しだけ内側(0.5f)を限界値にする
            return new Vector2(-halfWidth + 0.15f, halfWidth - 0.15f);
        }
        return new Vector2(-2.8f, 2.8f); // カメラがない場合の安全策
    }
}