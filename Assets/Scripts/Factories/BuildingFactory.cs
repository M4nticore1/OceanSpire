using UnityEngine;

public static class BuildingFactory
{
    public static TowerBuilding CreateBuilding(TowerBuilding prefab, Transform transform, TowerBuildingData data)
    {
        if (prefab == null) {
            Debug.LogError("Prefab is not valid!");
            return null;
        }

        var buildings = Object.Instantiate(prefab, transform.position, transform.rotation);
        buildings.Init(data);

        return buildings;
    }
}