using System;
using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }

    public int CurrentCraftingTime { get; private set; } = 0;
    public long? FinishTime { get; private set; } = null;
    public bool IsResourcesSpent { get; private set; } = false;

    public float CraftingSpeedMultiplier { get; private set; } = 1f;

    private CityStorage cityStorage => CityStorage.Instance;

    public event Action<float> OnSpeedBonusChanged;

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        SetFinishTime(data.CraftingFinishTime);
    }

    public int GetCraftTimeWithBonus()
    {
        return Mathf.Max(0, (int)(Definition.ProduceTime / CraftingSpeedMultiplier));
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
        CurrentCraftingTime = Mathf.Clamp(time, 0, Definition.ProduceTime);
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

    public void SetCraftingSpeedMultiplier(float multiplier)
    {
        CraftingSpeedMultiplier = Mathf.Max(0, multiplier);
        OnSpeedBonusChanged?.Invoke(multiplier);
    }

    public void SetResourcesSpent(bool value)
    {
        IsResourcesSpent = value;
    }

    public bool TrySpendResources()
    {
        if (IsResourcesSpent) return false;
        if (!cityStorage) return false;

        foreach (var resource in Definition.ConsumeResources) {
            cityStorage.Inventory.RemoveItemAmount(resource.Definition.ItemId, resource.Amount);
        }

        SetResourcesSpent(true);
        return true;
    }

    public bool TryRefundResources()
    {
        if (!IsResourcesSpent) return false;
        if (!cityStorage) return false;

        foreach (var resource in Definition.ConsumeResources) {
            cityStorage.Inventory.AddItemAmount(resource.Definition.ItemId, resource.Amount);
        }

        SetResourcesSpent(false);
        return true;
    }

    public long? GetFinishTimeWithBonus()
    {
        if (FinishTime == null) return null;

        float safeBonus = Mathf.Clamp01(CraftingSpeedMultiplier);
        int discountSeconds = (int)(Definition.ProduceTime * safeBonus);

        return FinishTime.Value - discountSeconds;
    }
}