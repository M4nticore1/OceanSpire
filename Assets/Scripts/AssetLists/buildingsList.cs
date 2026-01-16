using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "buildingPrefabsList", menuName = "GameContent/buildingPrefabsList")]
public class buildingsList : ScriptableObject
{
    [field: SerializeField] public Building[] buildings { get; private set; } = { };
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
