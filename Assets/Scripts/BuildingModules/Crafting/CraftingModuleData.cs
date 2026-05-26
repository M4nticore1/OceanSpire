using System;
using UnityEngine;

[Serializable]
public class CraftingModuleData
{
    public int CraftId = 0;
    public int CraftingTime = 0;

    public static CraftingModuleData Create(CraftingModule craftingModule)
    {
        if (!craftingModule) return null;

        return new CraftingModuleData()
        {
            CraftId = craftingModule.CurrentProductingItemIndex,
            CraftingTime = (int)craftingModule.CurrentProductionTime,
        };
    }
}