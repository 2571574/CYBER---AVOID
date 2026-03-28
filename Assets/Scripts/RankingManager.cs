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
    private const int MAX_RANKING_COUNT = 5; // 上位何名まで保存するか

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

    // ゲームオーバー時にスコアを送信し、ランクインしていれば保存する
    public void AddScoreAndSave(int newScore)
    {
        if (newScore <= 0) return;

        CurrentRanking.highScores.Add(newScore);

        // スコアを降順（大きい順）に並び替える
        CurrentRanking.highScores.Sort((a, b) => b.CompareTo(a));

        // MAX_RANKING_COUNT（5件）を超えたら、下位のスコアを削除する
        if (CurrentRanking.highScores.Count > MAX_RANKING_COUNT)
        {
            CurrentRanking.highScores.RemoveRange(MAX_RANKING_COUNT, CurrentRanking.highScores.Count - MAX_RANKING_COUNT);
        }

        // 最新のランキングデータをJSONテキストに変換して保存
        string json = JsonUtility.ToJson(CurrentRanking);
        PlayerPrefs.SetString(RANKING_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("ランキングを保存しました: " + json);
    }
}