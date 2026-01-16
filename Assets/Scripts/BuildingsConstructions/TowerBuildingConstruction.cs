using UnityEngine;

public class TowerBuildingConstruction : BuildingConstruction
{
    public int floorIndex => ((TowerBuilding)ownedBuilding).floorIndex;
    public int placeIndex => ((TowerBuilding)ownedBuilding).placeIndex;
}
