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

        return CreateBuilding_Internal(prefab, data);
    }

    public static TowerBuilding CreateBuilding(TowerBuilding prefab, TowerBuildingData data)
    {
        return CreateBuilding_Internal(prefab, data);
    }

    private static TowerBuilding CreateBuilding_Internal(TowerBuilding prefab, TowerBuildingData data)
    {
        var spawnedGO = Object.Instantiate(prefab);
        spawnedGO.Init(data);
        EventBus.InvokeBuildingCreated(spawnedGO);
        return spawnedGO;
    }
}
