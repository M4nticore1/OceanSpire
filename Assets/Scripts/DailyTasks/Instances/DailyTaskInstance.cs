using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DailyTaskInstance : ILocalizable
{
    public DailyTaskDefinition Definition { get; protected set; }
    public ItemInstance Reward { get; protected set; }
    public int ConditionAmount { get; protected set; }
    public int TaskProgress { get; protected set; } = 0;

    public bool IsCompleted => TaskProgress >= ConditionAmount;
    public float TaskProgressAlpha => ConditionAmount > 0 ? (float)TaskProgress / ConditionAmount : 1f;

    public int Id
    {
        get {
            var index = DailyTasksList.Instance.IndexOf(Definition);
            if (index == null) return 0;

            return index.Value;
        }
    }

    public int RewardId
    {
        get {
            var index = Array.IndexOf(Definition.RandomRewards, Reward);
            if (index < 0) return 0;

            return index;
        }
    }

    public event Action<int> OnProgressAdded;

    public static event Action<DailyTaskInstance, int> OnTaskProgressAdded;
    public static event Action<DailyTaskInstance> OnTaskCompleted;

    // Information
    public virtual LocalizationItem GetConditionName()
    {
        return null;
    }

    public virtual LocalizationItem GetConditionDescription()
    {
        return Definition.DescriptionLocalizationItem;
    }

    public virtual Sprite GetConditionIcon()
    {
        return null;
    }

    // ILocalizable
    public virtual Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"rewardName", LocalizationManager.Instance.GetLocalizedText(Reward?.Definition?.NameLocalizationItem).ToLower()},
            {"rewardAmount", Reward?.Amount.ToString()},
            {"taskCondition", ConditionAmount.ToString() + (GetConditionName() != null ? " " + LocalizationManager.Instance.GetLocalizedText(GetConditionName()).ToLower() : "")},
        };
    }

    public void AddProgress(int value)
    {
        if (IsCompleted) return;

        TaskProgress += value;
        TaskProgress = Mathf.Clamp(TaskProgress, 0, ConditionAmount);

        OnProgressAdded?.Invoke(value);
        OnTaskProgressAdded?.Invoke(this, value);

        TryComplete();
    }

    private bool TryComplete()
    {
        if (TaskProgress >= ConditionAmount) {
            Complete();
            return true;
        }

        return false;
    }

    private void Complete()
    {
        OnTaskCompleted?.Invoke(this);
    }
}