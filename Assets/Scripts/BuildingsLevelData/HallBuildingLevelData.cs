using UnityEngine;

[CreateAssetMenu(fileName = "HallLevelData", menuName = "Scriptable Objects/HallLevelData")]
public class HallBuildingLevelData : BuildingLevelData
{
    [Header("Construction")]
    public BuildingConstruction buildingConstruction;

    [Header("Vertical")]
    public BuildingConstruction buildingConstructionAbove;
    public BuildingConstruction buildingConstructionBelow;
    public BuildingConstruction buildingConstructionAboveBelow;
}
