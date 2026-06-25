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
        var rewardDefinition = RewardsList.Instance.GetRewardDefinition(Id);
        if (!rewardDefinition)
            return null;

        var reward = new RewardInstance(rewardDefinition, Amount);

        return reward;
    }

    public static RewardInstanceData[] CreateRewards(RewardInstance[] rewards)
    {
        var rewardsList = new List<RewardInstanceData>();

        foreach (var reward in rewards) {
            if (reward == null) {
                Debug.LogError($"reward is not valid");
                continue;
            }

            var rewardData = reward.CreateData();
            if (rewardData == null) {
                Debug.LogError($"rewardData is not valid");
                continue;
            }

            rewardsList.Add(rewardData);
        }

        return rewardsList.ToArray();
    }
}