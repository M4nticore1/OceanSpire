using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DailyTaskInstance : ILocalizable
{
    public DailyTaskDefinition Definition { get; protected set; }
    public ItemInstance Reward { get; protected set; }
    public int ConditionAmount { get; protected set; }
    public int TaskProgress { get; protected set; } = 0;
    public bool IsCompleted { get; protected set; } = false;

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

    public event Action OnProgressChanged;
    public event Action OnTaskRemoved;

    public static event Action<DailyTaskInstance, int> onTaskProgressAdded;
    public static event Action<DailyTaskInstance> onTaskCompleted;

    //public void RemoveTask()
    //{
    //    DailyTaskController.OnProgressChanged -= HandleProgressChanged;
    //    OnTaskRemoved?.Invoke();
    //}

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
    public Dictionary<string, string> GetLocalization()
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
        TaskProgress += value;
        onTaskProgressAdded?.Invoke(this, value);
    }

    //private void HandleProgressChanged(DailyTaskController controller, int value)
    //{
    //    if (controller == null) return;

    //    if (IsCompleted) return;
    //    if (!controller.DailyTaskDefinition != Definition) return;

    //    AddProgress(value);

    //    if (TryComplete()) {
    //        ReceiveReward();
    //    }

    //    OnProgressChanged?.Invoke();
    //}

    public bool TryComplete()
    {
        if (TaskProgress < ConditionAmount) return false;

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
        var id = Reward.Definition.ItemId;
        var amount = Reward.Amount;

        CityStorage.Instance.Inventory.AddItemAmount(id, amount);
    }
}