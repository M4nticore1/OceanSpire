using UnityEngine;

public static class BuildingFactory
{
    public static TowerBuilding CreateBuilding(int id, TowerBuildingEntry data)
    {
        TowerBuilding prefab = BuildingsList.Instance.buildingsDict[id] as TowerBuilding;
        if (!prefab) {
            Debug.LogError($"No prefab found for Building ID {id}");
            return null;
        }

        return CreateBuilding_Internal(prefab, data);
    }

    public static TowerBuilding CreateBuilding(TowerBuilding prefab, TowerBuildingEntry data)
    {
        return CreateBuilding_Internal(prefab, data);
    }

    private static TowerBuilding CreateBuilding_Internal(TowerBuilding prefab, TowerBuildingEntry data)
    {
        var obj = Object.Instantiate(prefab);
        obj.Init(data);
        return obj;
    }
}
