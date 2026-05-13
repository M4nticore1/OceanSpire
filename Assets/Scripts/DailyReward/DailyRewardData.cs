using System;
using UnityEngine;

[Serializable]
public class DailyRewardData
{
    public long NextResetTime = 0;
    public RewardInstanceData[] Rewards = null;

    public static DailyRewardData Create(DailyRewardManager dailyRewardManager)
    {
        var rewards = new RewardInstanceData[dailyRewardManager.CurrentRewards.Count];
        for (int i = 0; i < rewards.Length; i++) {
            var reward = dailyRewardManager.CurrentRewards[i];
            rewards[i] = reward.CreateData();
        }

        return new DailyRewardData()
        {
            NextResetTime = dailyRewardManager.NextResetTime,
            Rewards = rewards,
        };
    }
}
