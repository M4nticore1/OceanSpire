using UnityEngine;

public class DailyRewardLoader : Loader
{
    [SerializeField] DailyRewardManager dailyRewardManager;

    protected override void Load(WorldData worldData)
    {
        var dailyRewardData = worldData?.DailyReward;

        if (dailyRewardData != null && dailyRewardData != null)
            dailyRewardManager.Init(dailyRewardData);
        else
            dailyRewardManager.Init();
    }
}