using System.Collections.Generic;
using UnityEngine;

public abstract class AdRewardInstance : ILocalizable
{
    public AdRewardData rewardData { get; private set; }
    public bool isRemainable { get; private set; } = false;
    public float limitTime { get; private set; } = 0f;
    public float remainingTime { get; private set; } = 0f;

    protected Dictionary<string, string> localization = new Dictionary<string, string>();
    public Dictionary<string, string> Localization => localization;

    public AdRewardInstance(AdRewardData data)
    {
        rewardData = data;
    }

    protected abstract void OnRewardRecieved();
    protected abstract void AssignPlaceHoldersLocalization();

    public void RecieveReward()
    {
        OnRewardRecieved();
        EventBus.InvokeAdRewardRecieved(this);
    }

    public void SetLimitTime(float value)
    {
        limitTime = value;
        remainingTime = value;
    }

    public void ReduceRemainingTime(float value)
    {
        remainingTime -= value;
    }
}