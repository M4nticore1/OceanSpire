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
    public event Action onTaskRemoved;
    public static event Action<DailyTaskInstance, int> onTaskProgressAdded;
    public static event Action<DailyTaskInstance> onTaskCompleted;

    public DailyTaskInstance(DailyTaskDefinition definition, int progress)
    {
        Definition = definition;

        DailyTaskCondition.onProgressChanged += OnProgressChanged;
    }

    public void RemoveTask()
    {
        DailyTaskCondition.onProgressChanged -= OnProgressChanged;
        onTaskRemoved?.Invoke();
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"rewardName", LocalizationManager.Instance.GetText(Definition.Reward.Definition.NameLocalization).ToLower()},
            {"rewardAmount", Definition.Reward.Amount.ToString()},
            {"taskCondition", Definition.ConditionAmount.ToString() + (Definition.ConditionLocalizationItem ? " " + LocalizationManager.Instance.GetText(Definition.ConditionLocalizationItem).ToLower() : "")},
        };
    }

    protected void AddProgress(int value)
    {
        Progress += value;
        onTaskProgressAdded?.Invoke(this, value);
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
        onTaskCompleted?.Invoke(this);
    }

    private void ReceiveReward()
    {
        int id = Definition.Reward.Definition.ItemId;
        int amount = Definition.Reward.Amount;

        CityStorage.Instance.Inventory.AddItem(id, amount);
    }
}