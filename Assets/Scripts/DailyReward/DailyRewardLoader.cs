using UnityEngine;

public class DailyRewardLoader : Loader
{
    [SerializeField] DailyRewardManager dailyRewardManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.DailyReward != null) {
            LoadRewards(worldData.DailyReward);
        }
        else {
            InitRewards();
        }
    }

    private void LoadRewards(DailyRewardData dailyRewardData)
    {
        dailyRewardManager.Init(dailyRewardData);
    }

    private void InitRewards()
    {
        var dailyRewardData = new DailyRewardData()
        {
            Rewards = dailyRewardManager.GetRandomRewardsData(),
            NextResetTime = dailyRewardManager.CalculateNextResetTime(),
        };

        dailyRewardManager.Init(dailyRewardData);
    }
}