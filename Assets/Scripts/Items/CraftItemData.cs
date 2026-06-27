using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public long? CraftingFinishTime = null;
    public bool CraftingInProgress = false;

    public static CraftItemData Create(CraftItemInstance item)
    {
        return new CraftItemData()
        {
            CraftingFinishTime = item.CraftingFinishTime,
            CraftingInProgress = item.CraftingInProgress
        };
    }

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}