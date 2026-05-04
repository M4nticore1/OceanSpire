using System;
using System.Collections.Generic;

public abstract class AdRewardInstance : ILocalizable
{
    public AdRewardDefinition Definition { get; private set; }
    public bool IsRecieved { get; private set; } = false;

    public static event Action<AdRewardInstance> onRewardReceived;
    public static event Action<AdRewardInstance> onRewardRemoved;

    public AdRewardInstance()
    {

    }

    public AdRewardInstance(AdRewardDefinition definition)
    {
        Definition = definition;
    }

    public abstract Dictionary<string, string> GetLocalization();
    protected abstract void OnRewardRecieved();

    public void RecieveReward()
    {
        OnRewardRecieved();
        IsRecieved = true;
        onRewardReceived?.Invoke(this);
    }
}