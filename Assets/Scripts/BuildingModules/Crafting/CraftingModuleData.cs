using System;
using UnityEngine;

[Serializable]
public class CraftingModuleData
{
    public int CurrentCraftId = 0;
    public CraftItemData CurrentCraft = CraftItemData.Default();

    public static CraftingModuleData Create(CraftingModule craftingModule)
    {
        if (!craftingModule) return null;

        return new CraftingModuleData()
        {
            CurrentCraftId = craftingModule.GetIndexOfCurrentCraftItem(),
            CurrentCraft = craftingModule.CurrentCraftItem != null ? CraftItemData.Create(craftingModule.CurrentCraftItem) : CraftItemData.Default(),
        };
    }

    public static CraftingModuleData Default()
    {
        return new CraftingModuleData();
    }
}