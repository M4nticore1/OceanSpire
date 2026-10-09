using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacesManager : MonoBehaviour
{
    public static BuildingPlacesManager Instance { get; private set; }

    private List<BuildingPlace> buildingPlaces = new();
    public IReadOnlyList<BuildingPlace> BuildingPlaces => buildingPlaces;

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError("Another BuildingPlacesManager on the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterBuildingPlace(BuildingPlace buildingPlace)
    {
        if (buildingPlace == null) return;
        if (buildingPlaces.Contains(buildingPlace)) return;

        buildingPlaces.Add(buildingPlace);
    }

    public void UnregisterBuildingPlace(BuildingPlace buildingPlace)
    {
        buildingPlaces.Remove(buildingPlace);
    }
}