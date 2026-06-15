using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public int CurrentCraftingTime = 0;
    public bool CraftingInProgress = false;

    public static CraftItemData Create(CraftItemInstance item)
    {
        return new CraftItemData()
        {
            CurrentCraftingTime = (int)item.CurrentCraftingTime,
            CraftingInProgress = item.IsCrafting
        };
    }

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}