using System;
using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }
    public float CurrentCraftingTime { get; private set; } = 0f;
    public bool IsCrafting { get; private set; } = false;
    public float CraftingSpeedBonus { get; private set; } = 0f;

    public event Action<float> OnCraftingSpeedBonusChanged;

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        CurrentCraftingTime = data.CurrentCraftingTime;
    }

    public void SetCurrentCraftingTime(float time)
    {
        CurrentCraftingTime = time;
    }

    public void SetIsCrafting(bool value)
    {
        IsCrafting = value;
    }

    public void SetCraftingSpeedBonus(float value)
    {
        CraftingSpeedBonus = value;
        OnCraftingSpeedBonusChanged?.Invoke(value);
    }

    public bool IsCraftingFinished()
    {
        if (CurrentCraftingTime < GetProduceTime()) return false;

        return true;
    }

    public float GetProduceTime()
    {
        var produceTime = Definition.ProduceTime;
        var bonusMultiplier = (1 - CraftingSpeedBonus);
        var bonusProduceTime = produceTime * bonusMultiplier;

        return bonusProduceTime;
    }
}