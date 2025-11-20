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
    Right
}

[AddComponentMenu("")]
public class TowerBuilding : Building
{
    public TowerBuilding leftConnectedBuilding { get; private set; } = null;
    public TowerBuilding rightConnectedBuilding { get; private set; } = null;
    public TowerBuilding aboveConnectedBuilding { get; private set; } = null;
    public TowerBuilding belowConnectedBuilding { get; private set; } = null;

    protected override void Place()
    {
        leftConnectedBuilding = GetNeightboorBuilding(Side.Left);
        rightConnectedBuilding = GetNeightboorBuilding(Side.Right);

        base.Place();
    }

    private TowerBuilding GetNeightboorBuilding(Side side)
    {
        int indexOffset = side == Side.Left ? 1 : -1;

        int sideIndex = (GetPlaceIndex() + indexOffset + CityManager.roomsCountPerFloor) % (CityManager.roomsCountPerFloor - 1);
        TowerBuilding sideBuilding = cityManager.builtFloors[GetFloorIndex()].roomBuildingPlaces[sideIndex].placedBuilding as TowerBuilding;
        return sideBuilding;
    }
}
