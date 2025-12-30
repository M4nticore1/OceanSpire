using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingPrefabsList", menuName = "GameContent/BuildingPrefabsList")]
public class BuildingPrefabsList : ScriptableObject
{
    [SerializeField] private List<Building> buildingPrefabs = new List<Building>();
    public List<Building> BuildingPrefabs => buildingPrefabs;
    public Dictionary<int, Building> buildingPrefabsById { get; private set; } = new Dictionary<int, Building>();
    public Dictionary<string, Building> buildingPrefabsByKey { get; private set; } = new Dictionary<string, Building>();

    public void Initialize()
    {
        buildingPrefabsById.Clear();
        buildingPrefabsByKey.Clear();

        foreach (Building building in buildingPrefabs)
        {
            BuildingData data = building.BuildingData;
            if (building == null) {
                Debug.LogError("Building is NULL in list");
                continue; }

            int id = data.BuildingId;
            if (!buildingPrefabsById.TryAdd(id, building))
                Debug.LogError($"buildingPrefabsById already contains {id} id");

            string key = data.BuildingIdName;
            if (!buildingPrefabsByKey.TryAdd(key, building))
                Debug.LogError($"buildingPrefabsByKey already contains {key} id");
        }
    }

    //public Building GetBuildingPrefab(int buildingId)
    //{
    //    for (int i = 0; i < buildingPrefabs.Count; i++)
    //    {
    //        if (buildingPrefabs[i].BuildingData.BuildingId == buildingId)
    //        {
    //            return buildingPrefabs[i];
    //        }
    //    }

    //    return null;
    //}

    //public Building GetBuildingPrefab(string buildingIdName)
    //{
    //    for (int i = 0; i < buildingPrefabs.Count; i++)
    //    {
    //        if (buildingPrefabs[i].BuildingData.BuildingIdName == buildingIdName)
    //        {
    //            return buildingPrefabs[i];
    //        }
    //    }

    //    return null;
    //}
}
