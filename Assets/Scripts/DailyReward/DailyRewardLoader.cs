using UnityEngine;

public class DailyRewardLoader : WorldLoader
{
    [SerializeField] DailyRewardManager dailyRewardManager;

    protected override void Load(WorldData worldData)
    {
        var dailyRewardData = worldData?.DailyReward;

        if (dailyRewardData != null) {
            dailyRewardManager.Init(dailyRewardData);
        }
        else {
            dailyRewardManager.Init();
        }
    }
}