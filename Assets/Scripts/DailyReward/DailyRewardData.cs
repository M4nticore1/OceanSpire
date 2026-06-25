using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class DailyRewardData
{
    public RewardInstanceData[] Rewards = null;
    public long NextResetTime = 0;
    public bool FreeRewardCollected = false;
    public bool AdRewardCollected = false;
    public bool RewardViewed = false;

    public static DailyRewardData Create(DailyRewardManager dailyRewardManager)
    {
        if (!dailyRewardManager) {
            Debug.Log("Daily Reward Manager is not valid");
            return null;
        }

        return new DailyRewardData()
        {
            NextResetTime = dailyRewardManager.NextResetTime,
            Rewards = RewardInstanceData.CreateRewards(dailyRewardManager.CurrentRewards.ToArray()),
            FreeRewardCollected = dailyRewardManager.FreeRewardCollected,
            AdRewardCollected = dailyRewardManager.AdRewardCollected,
            RewardViewed = dailyRewardManager.IsRewardViewed,
        };
    }
}