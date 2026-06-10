using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RewardInstanceData
{
    public int Id = 0;
    public int Amount = 0;
    public bool Collected = false;

    public virtual RewardInstance CreateReward()
    {
        return null;
    }

    public static RewardInstanceData[] CreateRewards(RewardInstance[] rewards)
    {
        List<RewardInstanceData> rewardsList = new();

        foreach (var reward in rewards) {
            if (reward == null) {
                Debug.Log($"Daily Reward not found");
                continue;
            }

            rewardsList.Add(reward.CreateData());
        }

        return rewardsList.ToArray();
    }
}