using System;
using UnityEngine;

[Serializable]
public enum BuildingId
{
    TowerGate,
    FloorFrame,
    Dock,
    BasicElevator,
    FastElevator,
    HeavyElevator,
    LivingRooms,
    CoalGenerator,
    WoodStorage,
    MetalStorage,
    PlasticStorage,
    Lighthouse,
    Farm,
};

[Serializable]
public enum BuildingType
{
    Room,
    Hall,
    Elevator,
    FloorFrame,
    Environment,
};

[Serializable]
public enum BuildingCategory
{
    Construction,
    Residential,
    Production,
    Storage,
    Economy,
    Research,
}

public enum ConnectionType
{
    None,
    Horizontal,
    Vertical
}

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] private BuildingId buildingId = BuildingId.TowerGate;
    public BuildingId BuildingId => buildingId;
    [SerializeField] private string buildingIdName = "";
    public string BuildingIdName => buildingIdName;
    [SerializeField] private string buildingName = "";
    public string BuildingName => buildingName;
    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;
    [SerializeField] private BuildingCategory buildingCategory = BuildingCategory.Construction;
    public BuildingCategory BuildingCategory => buildingCategory;
    [SerializeField] private ConnectionType connectionType = ConnectionType.None;
    public ConnectionType ConnectionType => connectionType;
    [SerializeField] private bool instantConstruction = false;
    public bool InstantConstruction => instantConstruction;
    [SerializeField] private Sprite thumbImage = null;
    public Sprite ThumbImage => thumbImage;

    [SerializeField] private int buildingFloors = 1;
    public int BuildingFloors => buildingFloors;
    [SerializeField] private int maxBuildingFloors = 1;
    public int MaxBuildingFloors => maxBuildingFloors;

    [SerializeField] private bool isDemolishable = true;
    public bool IsDemolishable => isDemolishable;

    [Header("UI")]
    [SerializeField] private GameObject buildingManagementMenuWidget = null;
    public GameObject BuildingManagementMenuWidget => buildingManagementMenuWidget;
}
