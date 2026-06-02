using System;
using UnityEngine;

[Serializable]
public class DailyRewardData
{
    public RewardInstanceData[] Rewards = null;
    public long NextResetTime = 0;
    public bool FreeRewardCollected = false;
    public bool AdRewardCollected = false;

    public static DailyRewardData Create(DailyRewardManager dailyRewardManager)
    {
        var rewards = new RewardInstanceData[dailyRewardManager.CurrentRewards.Count];

        for (int i = 0; i < rewards.Length; i++) {
            var reward = dailyRewardManager.CurrentRewards[i];

            if (reward == null) {
                Debug.Log($"Reward by index {i} is not valid");
                continue;
            }

            rewards[i] = reward.CreateData();
        }

        return new DailyRewardData()
        {
            NextResetTime = dailyRewardManager.NextResetTime,
            Rewards = rewards,
        };
    }
}
