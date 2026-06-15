using UnityEngine;

public class CraftItemInstance
{
    public CraftItemDefinition Definition { get; private set; }
    public float CurrentCraftingTime { get; private set; }

    public CraftItemInstance(CraftItemDefinition definition, CraftItemData data)
    {
        Definition = definition;
        CurrentCraftingTime = data.CurrentCraftingTime;
    }

    public void SetCurrentCraftingTime(float time)
    {
        CurrentCraftingTime = time;
    }

    public bool IsReadyToCollect()
    {
        if (CurrentCraftingTime < Definition.ProduceTime) return false;

        return true;
    }
}