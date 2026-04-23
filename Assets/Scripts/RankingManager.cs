using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Core.Environments;

public class RankingManager : MonoBehaviour
{

    // UGSのDashboardで設定したリーダーボードのIDをここに入力します
    private const string LEADERBOARD_ID = "SCORE_RANKING";

    // スコアと名前をセットで保持する専用の構造体
    public struct ScoreData
    {
        public string PlayerName;
        public int Score;
    }

    // 取得したランキングデータを保持するリスト
    public List<ScoreData> CurrentRanking { get; private set; } = new List<ScoreData>();

    public bool IsPlayerNameSet { get; private set; } = false;

    private async void Start()
    {
        await InitializeUGSAsync();
    }

    private async Task InitializeUGSAsync()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }

            var options = new InitializationOptions();
            options.SetEnvironmentName("production");

            await UnityServices.InitializeAsync(options);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("サーバーへログイン成功: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("初期化エラー: " + e);
        }
    }

    public async Task ResetPlayerSessionAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;
        try
        {
            IsPlayerNameSet = false;

            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                AuthenticationService.Instance.ClearSessionToken();
            }
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e) { Debug.LogError("リセットエラー: " + e); }
    }

    // プレイヤー名をサーバーに登録・更新する
    public async Task UpdatePlayerNameAsync(string playerName)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;

        if (string.IsNullOrEmpty(playerName)) playerName = "player";

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            IsPlayerNameSet = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("名前更新エラー: " + e);
        }
    }

    // スコアを送信する（UIManagerのGameOver処理から呼ばれる）
    public async Task AddScoreAndSaveAsync(int score)
    {
        if (!IsPlayerNameSet)
        {
            Debug.LogWarning("名前の登録が未完了のため、スコアの送信をスキップしました。");
            return;
        }

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LEADERBOARD_ID, score);
            Debug.Log("スコア送信完了: " + score);
        }
        catch (System.Exception e)
        {
            Debug.LogError("スコア送信エラー: " + e);
        }
    }

    // サーバーからランキングを取得する
    public async Task<bool> FetchRankingAsync()
    {
        try
        {
            var options = new GetScoresOptions { Limit = 40 };
            // 上位のデータを取得
            var response = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, options);

            CurrentRanking.Clear();
            foreach (var entry in response.Results)
            {
                // UGSの仕様上、名前に「#数字」のIDが付与されるため、表示用にカットする
                string name = string.IsNullOrEmpty(entry.PlayerName) ? "Anonymous" : entry.PlayerName;
                if (name.Contains("#")) name = name.Split('#')[0];

                CurrentRanking.Add(new ScoreData
                {
                    PlayerName = name,
                    Score = (int)entry.Score
                });
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("ランキング取得エラー: " + e);
            return false;
        }
    }
}