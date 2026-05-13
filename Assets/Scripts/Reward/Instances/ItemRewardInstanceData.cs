using System;
using UnityEngine;

[Serializable]
public class ItemRewardInstanceData : RewardInstanceData
{
    public int Amount = 0;

    public override RewardInstance CreateReward()
    {
        var rewardDefinition = RewardsList.Instance.GetRewardDefinition(Id) as ItemAdRewardDefinition;
        if (!rewardDefinition)
            return null;

        var reward = new ItemRewardInstance(rewardDefinition, Amount);

        return reward;
    }

    public static ItemRewardInstanceData Create(ItemRewardInstance reward)
    {
        return new ItemRewardInstanceData()
        {
            Id = (int)reward.Definition.RewardId,
            Collected = reward.IsCollected,
            Amount = reward.Amount
        };
    }
}