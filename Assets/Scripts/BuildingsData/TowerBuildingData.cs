using System;
using UnityEngine;

[Serializable]
public class TowerBuildingData : BuildingData
{
    public int FloorIndex = 0;
    public int PlaceIndex = 0;

    public static TowerBuildingData Create(TowerBuilding building)
    {
        TowerBuildingData data = new TowerBuildingData();

        data.Fill(building);

        data.FloorIndex = building.FloorIndex;
        data.PlaceIndex = building.PlaceIndex;

        return data;
    }
}