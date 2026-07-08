using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyTaskInstance : ILocalizable
{
    public DailyTaskDefinition Definition { get; private set; }
    public int Id { get; private set; } = 0;
    public int Progress { get; private set; } = 0;
    public bool IsCompleted { get; private set; } = false;

    public event Action OnProgressChanged;
    public event Action OnTaskRemoved;
    public static event Action<DailyTaskInstance, int> onTaskProgressAdded;
    public static event Action<DailyTaskInstance> onTaskCompleted;

    public DailyTaskInstance(DailyTaskDefinition definition, int id, int progress, bool completed)
    {
        Definition = definition;
        Id = id;
        Progress = progress;
        IsCompleted = completed;

        DailyTaskCondition.OnProgressChanged += HandleProgressChanged;
    }

    public void RemoveTask()
    {
        DailyTaskCondition.OnProgressChanged -= HandleProgressChanged;
        OnTaskRemoved?.Invoke();
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

    private void HandleProgressChanged(DailyTaskCondition condition, int value)
    {
        if (IsCompleted) return;
        if (!condition.Definitions.Contains(Definition)) return;

        AddProgress(value);

        if (TryComplete()) {
            ReceiveReward();
        }

        OnProgressChanged?.Invoke();
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
        var id = Definition.Reward.Definition.ItemId;
        var amount = Definition.Reward.Amount;

        CityStorage.Instance.Inventory.AddItem(id, amount);
    }
}