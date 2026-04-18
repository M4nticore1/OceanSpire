using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingsList", menuName = "Game Content/building Prefabs List")]
public class BuildingsList : ScriptableObject
{
    private static BuildingsList _instance;
    public static BuildingsList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<BuildingsList>("Lists/BuildingsList");
            }

            return _instance;
        }
    }

    [SerializeField] private Building[] buildings = null;
    public Building[] Buildings => buildings;

    public Dictionary<int, Building> buildingsDict = new Dictionary<int, Building>();

    public void Init()
    {
        foreach (Building building in buildings) {
            buildingsDict.Add(building.BuildingData.BuildingId, building);
        }
    }
    //public Dictionary<int, Building> buildingsById { get; private set; } = new Dictionary<int, Building>();

    //public void Initialize()
    //{
    //    buildingsById.Clear();

    //    foreach (Building building in buildings)
    //    {
    //        BuildingData data = building.BuildingData;
    //        if (building == null) {
    //            Debug.LogError("Building is NULL in list");
    //            continue; }

    //        int id = data.BuildingId;
    //        if (!buildingsById.TryAdd(id, building))
    //            Debug.LogError($"buildingPrefabsById already contains {id} id");
    //    }
    //}

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
