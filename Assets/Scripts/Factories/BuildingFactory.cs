using System.Collections.Generic;
using UnityEngine;

public static class BuildingFactory
{
    public static TowerBuilding CreateBuilding(TowerBuilding prefab, TowerBuildingData data)
    {
        var go = Object.Instantiate(prefab);
        go.Init(data);

        return go;
    }
}
