using System;
using UnityEngine;

[Serializable]
public class RewardInstanceData
{
    public int Id = 0;
    public bool Collected = false;

    public virtual RewardInstance CreateReward()
    {
        return null;
    }

    public static RewardInstanceData CreateData(RewardInstance reward)
    {
        return new RewardInstanceData()
        {
            Id = (int)reward.Definition.RewardId,
            Collected = reward.IsCollected
        };
    }
}