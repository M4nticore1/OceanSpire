using System.Collections.Generic;
using UnityEngine;

public static class BuildingFactory
{
    public static TowerBuilding CreateBuilding(TowerBuilding prefab, Transform transform, TowerBuildingData data)
    {
        var buildings = Object.Instantiate(prefab, transform.position, transform.rotation);
        buildings.Init(data);

        return buildings;
    }
}