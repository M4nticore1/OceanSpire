using UnityEngine;

[CreateAssetMenu(fileName = "ProductionBuildingLevelData", menuName = "Scriptable Objects/ProductionBuildingLevelData")]
public class ProductionModuleLevelData : BuildingModuleLevelData
{
    [SerializeField] private CraftItem[] craftItems;
    public CraftItem[] CraftItems => craftItems;
}
