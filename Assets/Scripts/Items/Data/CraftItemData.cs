using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public long? CraftingFinishTime = null;
    public int CurrentCraftingTime = 0;
    public bool ResourcesSpent = false;

    public static CraftItemData Create(CraftItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(CraftItemData)}] Item is not valid!");
            return Default();
        }

        return new CraftItemData()
        {
            CraftingFinishTime = item.FinishTime,
            CurrentCraftingTime = item.CurrentCraftingTime,
            ResourcesSpent = item.IsResourcesSpent
        };
    }

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}