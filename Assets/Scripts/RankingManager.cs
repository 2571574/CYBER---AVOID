using System.Collections.Generic;
using UnityEngine;

// JSONで保存するためのデータ構造（構造体）
[System.Serializable]
public class RankingData
{
    // トップ5のスコアを保存するリスト
    public List<int> highScores = new List<int>();
}

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }

    private const string RANKING_KEY = "LocalRankingData";
    private const int MAX_RANKING_COUNT = 30; // 上位何名まで保存するか

    public RankingData CurrentRanking { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadRanking();
    }

    // 起動時にローカルからランキングを読み込む
    private void LoadRanking()
    {
        if (PlayerPrefs.HasKey(RANKING_KEY))
        {
            string json = PlayerPrefs.GetString(RANKING_KEY);
            CurrentRanking = JsonUtility.FromJson<RankingData>(json);
        }
        else
        {
            CurrentRanking = new RankingData();
        }
    }

    // ゲームオーバー時にスコアを保存し、ランクインしていればインデックスで返す
    public int AddScoreAndSave(int newScore)
    {
        if (newScore <= 0) return -1;

        int rankIndex = -1;

        for (int i = 0; i < CurrentRanking.highScores.Count; i++)
        {
            if (newScore > CurrentRanking.highScores[i])
            {
                rankIndex = i;
                break;
            }
        }

        if (rankIndex == -1 && CurrentRanking.highScores.Count < MAX_RANKING_COUNT)
        {
            rankIndex = CurrentRanking.highScores.Count;
        }

        if (rankIndex == -1) return -1;

        CurrentRanking.highScores.Insert(rankIndex, newScore);

        if (CurrentRanking.highScores.Count > MAX_RANKING_COUNT)
        {
            CurrentRanking.highScores.RemoveAt(CurrentRanking.highScores.Count - 1);
        }

        string json = JsonUtility.ToJson(CurrentRanking);
        PlayerPrefs.SetString(RANKING_KEY, json);
        PlayerPrefs.Save();

        return rankIndex;
    }
}