using System;
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
}