using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum BuildingPosition
{
    Straight,
    Corner
}

public enum Side
{
    Left,
    Right,
    Up,
    Down
}

[AddComponentMenu("")]
public class TowerBuilding : Building
{
    //public TowerBuilding leftConnectedBuilding = null;
    //public TowerBuilding rightConnectedBuilding = null;
    //public TowerBuilding aboveConnectedBuilding = null;
    //public TowerBuilding belowConnectedBuilding = null;

    public override void InitializeBuilding(BuildingPlace buildingPlace, bool isUnderConstruction, int levelIndex, int interiorIndex = -1)
    {
        base.InitializeBuilding(buildingPlace, isUnderConstruction, levelIndex, interiorIndex);

        //leftConnectedBuilding = GetNeightboorBuilding(Side.Left);
        //rightConnectedBuilding = GetNeightboorBuilding(Side.Right);
        //aboveConnectedBuilding = GetNeightboorBuilding(Side.Up);
        //belowConnectedBuilding = GetNeightboorBuilding(Side.Down);

        //if (GetplaceIndex % 2 == 0)
        //    buildingPosition = BuildingPosition.Corner;
        //else
        //    buildingPosition = BuildingPosition.Straight;
    }

    //private TowerBuilding GetNeightboorBuilding(Side side)
    //{
    //    int horizontalIndexOffset = side == Side.Left ? 1 : side == Side.Right ? -1 : 0;
    //    int verticalIndexOffset = side == Side.Up ? 1 : side == Side.Down ? -1 : 0;
    //    int sideIndex = (GetplaceIndex + horizontalIndexOffset + CityManager.roomsCountPerFloor) % CityManager.roomsCountPerFloor;
    //    int verticalIndex = GetfloorIndex + verticalIndexOffset;
    //    if (verticalIndex < cityManager.builtFloors.Count && verticalIndex >= 0)
    //        return cityManager.builtFloors[verticalIndex].roomBuildingPlaces[sideIndex].placedBuilding as TowerBuilding;

    //    return null;
    //}
}
