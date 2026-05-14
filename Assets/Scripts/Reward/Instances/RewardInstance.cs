using System;
using System.Collections.Generic;

public abstract class RewardInstance : ILocalizable
{
    public AdRewardDefinition Definition { get; private set; }
    public bool IsCollected { get; private set; } = false;

    public static event Action<RewardInstance> onRewardReceived;
    public static event Action<RewardInstance> onRewardRemoved;

    public RewardInstance()
    {

    }

    public RewardInstance(AdRewardDefinition definition)
    {
        Definition = definition;
    }

    public abstract Dictionary<string, string> GetLocalization();

    protected virtual void OnRewardRecieved()
    {
        IsCollected = true;
    }

    public void RecieveReward()
    {
        OnRewardRecieved();
        onRewardReceived?.Invoke(this);
    }

    public void SetCollected(bool value)
    {
        IsCollected = value;
    }

    public virtual RewardInstanceData CreateData()
    {
        return new RewardInstanceData() {
            Id = (int)Definition.RewardId,
            Collected = IsCollected,
        };
    }
}