using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public long? CraftingFinishTime = null;
    public bool Crafted = false;
    public bool CraftingInProgress = false;

    public static CraftItemData Create(CraftItemInstance item)
    {
        return new CraftItemData()
        {
            CraftingFinishTime = item.CraftingFinishTime,
            Crafted = item.IsCrafted,
        };
    }

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}