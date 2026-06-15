using System;
using UnityEngine;

[Serializable]
public class CraftItemData
{
    public int CurrentCraftingTime = 0;

    public static CraftItemData Default()
    {
        return new CraftItemData();
    }
}