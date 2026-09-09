using System;
using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }

    public int CraftTime => Definition != null ? Definition.ProduceTime : 0;
    public int CurrentCraftingTime { get; private set; } = 0;
    public long? FinishTime { get; private set; } = null;
    public bool IsResourcesSpent { get; private set; } = false;

    public float WorkerSkillsCraftingSpeedBonus { get; private set; } = 0f;
    public float EnergyShortageCraftingTimeCoefficient { get; private set; } = 1f;

    private CityStorage cityStorage => CityStorage.Instance;

    public event Action OnCraftingSpeedTimeChanged;

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        SetFinishTime(data.CraftingFinishTime);
    }

    public int GetCraftTimeWithBonus()
    {
        var bonusPercent = GetCraftingTimeBonusPercent();

        return Mathf.RoundToInt(CraftTime * (1f - bonusPercent));
    }

    public int GetRemainingCraftingTimeByCraftingTime()
    {
        return GetCraftTimeWithBonus() - CurrentCraftingTime;
    }

    public bool IsCraftingFinished()
    {
        return CurrentCraftingTime >= GetCraftTimeWithBonus();
    }

    public void UpdateCraftingTimeByFinishTime()
    {
        if (FinishTime == null) {
            SetCraftingTime(CurrentCraftingTime);
            return;
        }

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var startTime = FinishTime.Value - Definition.ProduceTime;
        var passedBaseSeconds = currentTime - startTime;
        var calculatedCraftingTime = (int)Mathf.Clamp(passedBaseSeconds, 0, Definition.ProduceTime);

        SetCraftingTime(calculatedCraftingTime);
    }

    public void SetCraftingTime(int time)
    {
        CurrentCraftingTime = Mathf.Clamp(time, 0, GetCraftTimeWithBonus());
    }

    public void ResetFinishTimeByCurrentCraftingTime()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var remainingBaseTime = Definition.ProduceTime - CurrentCraftingTime;

        SetFinishTime(currentTime + remainingBaseTime);
    }

    public void SetFinishTime(long? seconds)
    {
        FinishTime = seconds;
    }

    // Crafting Time
    public void SetCraftingSpeedMultiplier(float bonus)
    {
        var oldProgress = GetCraftingProgress();
        WorkerSkillsCraftingSpeedBonus = Mathf.Max(0, bonus);

        UpdateFinishTimeByCurrentProgress(oldProgress);

        OnCraftingSpeedTimeChanged?.Invoke();
    }

    public void SetEnergyShortageCraftingTimeCoefficient(float coefficient)
    {
        var oldProgress = GetCraftingProgress();
        EnergyShortageCraftingTimeCoefficient = Mathf.Max(0, coefficient);

        UpdateFinishTimeByCurrentProgress(oldProgress);

        OnCraftingSpeedTimeChanged?.Invoke();
    }

    public void SetResourcesSpent(bool value)
    {
        IsResourcesSpent = value;
    }

    public bool TrySpendResources()
    {
        if (IsResourcesSpent) return false;
        if (cityStorage == null) return false;

        foreach (var resource in Definition.ConsumeResources) {
            cityStorage.Inventory.RemoveItemAmount(resource.Definition.ItemId, resource.Amount);
        }

        SetResourcesSpent(true);
        return true;
    }

    public bool TryRefundResources()
    {
        if (!IsResourcesSpent) return false;
        if (cityStorage == null) return false;

        foreach (var resource in Definition.ConsumeResources) {
            cityStorage.Inventory.AddItemAmount(resource.Definition.ItemId, resource.Amount);
        }

        SetResourcesSpent(false);
        return true;
    }

    public float GetCraftingProgress()
    {
        return Mathf.Clamp01((float)CurrentCraftingTime / Definition.ProduceTime);
    }

    public float GetCraftingTimeBonusPercent()
    {
        var energyShortageCoefficient = 1f - EnergyShortageCraftingTimeCoefficient;

        return WorkerSkillsCraftingSpeedBonus - energyShortageCoefficient;
    }

    public long? GetFinishTimeWithBonus()
    {
        if (FinishTime == null) return null;

        var bonus = GetCraftingTimeBonusPercent();
        var timeChangeSeconds = Mathf.RoundToInt(Definition.ProduceTime * bonus);

        return FinishTime.Value - timeChangeSeconds;
    }

    private void UpdateFinishTimeByCurrentProgress(float oldProgress)
    {
        var time = Mathf.RoundToInt(GetCraftTimeWithBonus() * oldProgress);
        SetCraftingTime(time);
    }
}