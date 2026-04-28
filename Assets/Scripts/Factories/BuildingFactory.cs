using System.Collections.Generic;
using UnityEngine;

public static class BuildingFactory
{
    public static TowerBuilding CreateBuilding(TowerBuildingData data)
    {
        TowerBuilding prefab = BuildingsList.Instance.GetBuilding(data.Id) as TowerBuilding;
        if (!prefab) {
            Debug.LogError($"No prefab found for Building ID {data.Id}");
            return null;
        }

        return CreateBuilding(prefab, data);
    }

    public static TowerBuilding CreateBuilding(TowerBuilding prefab, TowerBuildingData data)
    {
        var go = Object.Instantiate(prefab);
        go.Init(data);

        return go;
    }
}
