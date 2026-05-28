using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingsList", menuName = "Game Content/building Prefabs List")]
public class BuildingsList : ScriptableObject
{
    private static BuildingsList instance;
    public static BuildingsList Instance
    {
        get
        {
            if (instance == null) {
                instance = Resources.Load<BuildingsList>("Lists/BuildingsList");
                instance.Init();
            }

            return instance;
        }
    }

    [SerializeField] private Building[] buildings = null;
    public Building[] Buildings => buildings;

    private Dictionary<int, Building> buildingsDict = new Dictionary<int, Building>();

    private void Init()
    {
        foreach (Building building in buildings) {
            buildingsDict.Add(building.BuildingData.BuildingId, building);
        }
    }

    public Building GetBuilding(int id)
    {
        buildingsDict.TryGetValue(id, out var building);

        return building;
    }
}