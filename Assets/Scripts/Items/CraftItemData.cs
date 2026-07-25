using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public long? CraftingFinishTime = null;

    public static CraftItemData Create(CraftItemInstance item)
    {
        return new CraftItemData()
        {
            CraftingFinishTime = item.FinishTime,
        };
    }

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}