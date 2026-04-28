using System;
using UnityEngine;

[Serializable]
public class TowerBuildingData : BuildingData
{
    public int FloorIndex { get; private set; } = 0;
    public int PlaceIndex { get; private set; } = 0;

    public TowerBuildingData(int id, int instanceId, int level, ConstructionData constructionData, int floorIndex, int placeIndex) : base(id, instanceId, level, constructionData)
    {
        FloorIndex = floorIndex;
        PlaceIndex = placeIndex;
    }
}
