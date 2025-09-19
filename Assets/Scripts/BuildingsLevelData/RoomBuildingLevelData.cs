using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelData", menuName = "Scriptable Objects/RoomLevelData")]
public class RoomBuildingLevelData : BuildingLevelData
{
    [Header("Non Connected")]
    public BuildingConstruction buildingStraight;
    public BuildingConstruction BuildingCorner;

    [Header("Horizontal")]
    [Header("Front")]
    public BuildingConstruction buildingStraightLeft;
    public BuildingConstruction buildingStraightRight;
    public BuildingConstruction buildingStraightLeftRight;

    [Header("Corner")]
    public BuildingConstruction buildingCornerLeft;
    public BuildingConstruction buildingCornerRight;
    public BuildingConstruction buildingCornerLeftRight;

    [Header("Vertical")]
    [Header("Front")]
    public BuildingConstruction buildingStraightAbove;
    public BuildingConstruction buildingStraightBelow;
    public BuildingConstruction buildingStraightAboveBelow;

    [Header("Corner")]
    public BuildingConstruction buildingCornerAbove;
    public BuildingConstruction buildingCornerBelow;
    public BuildingConstruction buildingCornerAboveBelow;
}
