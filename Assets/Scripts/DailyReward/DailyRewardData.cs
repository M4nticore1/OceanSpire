using System;
using System.Collections.Generic;
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
        if (!dailyRewardManager) {
            Debug.Log("Daily Reward Manager not found");
            return null;
        }

        return new DailyRewardData()
        {
            NextResetTime = dailyRewardManager.NextResetTime,
            Rewards = CreateRewards(dailyRewardManager),
        };
    }

    private static RewardInstanceData[] CreateRewards(DailyRewardManager dailyRewardManager)
    {
        if (dailyRewardManager.CurrentRewards == null) {
            Debug.Log("Current Rewards not found");
            return null;
        }

        List<RewardInstanceData> rewards = new();

        foreach (var reward in dailyRewardManager.CurrentRewards) {
            if (reward == null) {
                Debug.Log($"Daily Reward not found");
                continue;
            }

            rewards.Add(reward.CreateData());
        }

        return rewards.ToArray();
    }
}