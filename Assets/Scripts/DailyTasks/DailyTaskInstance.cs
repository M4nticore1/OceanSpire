using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyTaskInstance : ILocalizable
{
    public DailyTaskDefinition Definition { get; private set; }
    public int Progress { get; private set; } = 0;
    public bool IsCompleted { get; private set; } = false;

    public event Action onProgressChanged;

    public DailyTaskInstance(DailyTaskDefinition definition, int progress)
    {
        Definition = definition;

        DailyTaskCondition.onProgressChanged += OnProgressChanged;
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"rewardName", LocalizationManager.Instance.GetText(Definition.Reward.ItemData.LocalizationItem).ToLower()},
            {"rewardAmount", Definition.Reward.Amount.ToString()},
            {"taskCondition", Definition.ConditionAmount.ToString() + (Definition.ConditionLocalizationItem ? " " + LocalizationManager.Instance.GetText(Definition.ConditionLocalizationItem).ToLower() : "")},
        };
    }

    protected void AddProgress(int value)
    {
        Progress += value;
    }

    private void OnProgressChanged(DailyTaskCondition condition, int value)
    {
        if (IsCompleted) return;
        if (!condition.Definitions.Contains(Definition)) return;

        AddProgress(value);

        if (TryComplete()) {
            ReceiveReward();
        }

        onProgressChanged?.Invoke();
    }

    private bool TryComplete()
    {
        if (Progress < Definition.ConditionAmount) return false;

        Complete();
        return true;
    }

    private void Complete()
    {
        IsCompleted = true;
    }

    private void ReceiveReward()
    {
        int id = Definition.Reward.ItemData.ItemId;
        int amount = Definition.Reward.Amount;

        CityStorage.Instance.Inventory.AddItemAmount(id, amount);
    }
}