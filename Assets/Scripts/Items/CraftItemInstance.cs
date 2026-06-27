using System;
using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }

    public long? CraftingFinishTime { get; private set; } = 0;
    public bool CraftingInProgress { get; private set; } = false;
    public bool IsCraftSelected { get; private set; } = false;

    public float CraftingSpeedBonus { get; private set; } = 0f;

    public event Action<float> OnCraftingSpeedBonusChanged;

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        SetCraftingFinishTime(data.CraftingFinishTime);
        SetCraftingInProgress(data.CraftingInProgress);

        if (IsCraftingFinished()) {
            SetCraftingInProgress(false);
        }
    }

    public void ResetCraftingFinishTime()
    {
        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var craftingTime = Definition.ProduceTime;
        var finishTime = currentTime + craftingTime;

        SetCraftingFinishTime(finishTime);
    }

    public void RemoveCraftingFinishTime()
    {
        SetCraftingFinishTime(null);
    }

    public void SetCraftingFinishTime(long? time)
    {
        CraftingFinishTime = time;
    }

    public void SetCraftingInProgress(bool value)
    {
        CraftingInProgress = value;
    }

    public void SetCraftSelected(bool value)
    {
        IsCraftSelected = value;
    }

    public void SetCraftingSpeedBonus(float value)
    {
        CraftingSpeedBonus = value;
        OnCraftingSpeedBonusChanged?.Invoke(value);
    }

    public bool IsCraftingFinished()
    {
        if (CraftingFinishTime == null) return false;

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime < CraftingFinishTime.Value) return false;

        return true;
    }

    public int GetProduceTime()
    {
        var produceTime = Definition.ProduceTime;
        var bonusMultiplier = (1 - CraftingSpeedBonus);
        var bonusProduceTime = produceTime * bonusMultiplier;

        return (int)bonusProduceTime;
    }
}