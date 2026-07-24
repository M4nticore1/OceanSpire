using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TowerBuildingData : BuildingData
{
    public int FloorIndex = 0;
    public int PlaceIndex = 0;

    public static TowerBuildingData Create(TowerBuilding building)
    {
        var data = new TowerBuildingData();
        data.Fill(building);

        data.FloorIndex = building.FloorIndex;
        data.PlaceIndex = building.PlaceIndex;

        return data;
    }

    public static List<TowerBuildingData> Create(IReadOnlyList<TowerBuilding> buildings)
    {
        var buildingsData = new List<TowerBuildingData>();

        foreach (var building in buildings) {
            if (!building) continue;

            var data = Create(building);
            if (data == null) continue;

            buildingsData.Add(data);
        }

        return buildingsData;
    }
}