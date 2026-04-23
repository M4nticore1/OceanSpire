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

    private Dictionary<int, Building> buildingsDict = new Dictionary<int, Building>();

    public void Init()
    {
        foreach (Building building in buildings) {
            buildingsDict.Add(building.BuildingData.BuildingId, building);
        }
    }

    public Building GetBuilding(int id)
    {
        Building building;
        buildingsDict.TryGetValue(id, out building);

        return building;
    }
}