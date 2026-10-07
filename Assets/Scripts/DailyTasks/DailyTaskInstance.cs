using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DailyTaskInstance : ILocalizable
{
    public DailyTaskDefinition Definition { get; private set; }
    public ItemInstance Reward { get; private set; }
    public int Progress { get; private set; } = 0;
    public bool IsCompleted { get; private set; } = false;

    public int Id {
        get {
            var index = DailyTasksList.Instance.IndexOf(Definition);
            if (index == null) return 0;

            return index.Value;
        }
    }

    public int RewardId {
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

    public DailyTaskInstance(DailyTaskDefinition definition, ItemInstance reward, int progress, bool completed)
    {
        if (definition == null) {
            Debug.LogError("Definition is not valid!");
        }
        if (reward == null) {
            Debug.LogError("Reward is not valid!");
        }

        Definition = definition;
        Reward = reward;
        Progress = progress;
        IsCompleted = completed;

        DailyTaskController.OnProgressChanged += HandleProgressChanged;
    }

    public void RemoveTask()
    {
        DailyTaskController.OnProgressChanged -= HandleProgressChanged;
        OnTaskRemoved?.Invoke();
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            {"rewardName", LocalizationManager.Instance.GetLocalizedText(Reward?.Definition?.NameLocalizationItem).ToLower()},
            {"rewardAmount", Reward?.Amount.ToString()},
            {"taskCondition", Definition?.ConditionAmount.ToString() + (Definition?.ConditionLocalizationItem ? " " + LocalizationManager.Instance.GetLocalizedText(Definition?.ConditionLocalizationItem).ToLower() : "")},
        };
    }

    protected void AddProgress(int value)
    {
        Progress += value;
        onTaskProgressAdded?.Invoke(this, value);
    }

    private void HandleProgressChanged(DailyTaskController controller, int value)
    {
        if (controller == null) return;

        if (IsCompleted) return;
        if (!controller.DailyTaskDefinition != Definition) return;

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
        var id = Reward.Definition.ItemId;
        var amount = Reward.Amount;

        CityStorage.Instance.Inventory.AddItemAmount(id, amount);
    }
}