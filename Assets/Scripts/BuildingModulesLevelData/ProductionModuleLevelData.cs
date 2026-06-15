using UnityEngine;

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionModuleLevelData : BuildingModuleLevelData
{
    [SerializeField] private CraftItemDefinition[] craftItems;
    public CraftItemDefinition[] CraftItems => craftItems;

    public CraftItemDefinition TryGetCraftItem(int index)
    {
        if (craftItems.Length <= index) return null;

        return craftItems[index];
    }
}
