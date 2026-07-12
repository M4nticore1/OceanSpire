using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardInstance : ILocalizable
{
    public AdRewardDefinition Definition { get; private set; }
    public bool IsCollected { get; private set; } = false;
    public int Amount { get; private set; } = 0;

    protected Dictionary<string, string> localizationDictionary = new();

    public static event Action<RewardInstance> OnRewardReceived;
    public static event Action<RewardInstance> OnRewardRemoved;

    public RewardInstance()
    {

    }

    public RewardInstance(AdRewardDefinition definition, int amount)
    {
        Definition = definition;
        Amount = amount;
    }

    public Dictionary<string, string> GetLocalization()
    {
        localizationDictionary.Add("rewardName", LocalizationManager.Instance.GetLocalizedText(Definition.RewardNameLocalization).ToLower());
        localizationDictionary.Add("rewardAmount", Amount.ToString());

        return localizationDictionary;
    }

    protected virtual void OnRewardRecieved()
    {
        IsCollected = true;
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
    }

    public void RecieveReward()
    {
        OnRewardRecieved();
        OnRewardReceived?.Invoke(this);
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