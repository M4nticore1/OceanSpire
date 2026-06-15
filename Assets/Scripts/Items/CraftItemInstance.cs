using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }
    public float CurrentCraftingTime { get; private set; } = 0f;
    public bool IsCrafting { get; private set; } = false;

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

    public bool IsCraftingFinished()
    {
        if (CurrentCraftingTime < Definition.ProduceTime) return false;

        return true;
    }
}