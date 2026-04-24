using System;
using System.Collections.Generic;

public abstract class AdRewardInstance : ILocalizable
{
    public AdRewardDefinition definition { get; private set; }

    public static event Action<AdRewardInstance> onRewardReceived;
    public static event Action<AdRewardInstance> onRewardRemoved;

    public AdRewardInstance()
    {

    }

    public AdRewardInstance(AdRewardDefinition definition)
    {
        this.definition = definition;
    }

    public abstract Dictionary<string, string> GetLocalization();
    protected abstract void OnRewardRecieved();

    public void RecieveReward()
    {
        OnRewardRecieved();
        onRewardReceived?.Invoke(this);
    }
}