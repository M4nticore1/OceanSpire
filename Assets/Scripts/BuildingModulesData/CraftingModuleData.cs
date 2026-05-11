using UnityEngine;

public class CraftingModuleData
{
    public static CraftingModuleData Create(ProductionModule module)
    {
        if (!module) {
            return null;
        }

        return new CraftingModuleData()
        {

        };
    }
}