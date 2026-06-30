using System;
using UnityEngine;

[Serializable]
public class CraftingModuleData
{
    public int CurrentCraftId = 0;
    public CraftItemData SelectedCraft = CraftItemData.Default();

    public static CraftingModuleData Create(CraftingModule craftingModule)
    {
        if (!craftingModule) return null;

        return new CraftingModuleData()
        {
            CurrentCraftId = craftingModule.GetIndexOfCurrentCraftItem(),
            SelectedCraft = craftingModule.SelectedCraftItem != null ? CraftItemData.Create(craftingModule.SelectedCraftItem) : CraftItemData.Default(),
        };
    }

    public static CraftingModuleData Default()
    {
        return new CraftingModuleData();
    }
}