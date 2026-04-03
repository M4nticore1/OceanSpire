using System;
using UnityEngine;

public enum BuildingIdEnum
{
    FloorFrame,
    TowerGate,
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

public enum BuildingType
{
    Room,
    Hall,
    FloorFrame,
    Environment
};

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

public enum DetailsWindowVariant
{
    Building,
    ProductionBuilding,
    StorageBuilding
}

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Id")]
    [SerializeField] private BuildingIdEnum buildingId = BuildingIdEnum.TowerGate;
    public int BuildingId { get { return (int)buildingId; } }

    [SerializeField] private string buildingName = "";
    public string BuildingName => buildingName;

    [SerializeField] private LocalizationItem localizationItem = null;
    public LocalizationItem LocalizationItem => localizationItem;

    [Header("Enums")]
    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;

    [SerializeField] private BuildingCategory buildingCategory = BuildingCategory.Construction;
    public BuildingCategory BuildingCategory => buildingCategory;

    [SerializeField] private ConnectionType connectionType = ConnectionType.None;
    public ConnectionType ConnectionType => connectionType;

    [SerializeField] private BuildingStrategyEnum buildingStrategy = BuildingStrategyEnum.WorkBuilding;
    public BuildingStrategyEnum BuildingStrategy => buildingStrategy;

    [SerializeField] private DetailsWindowVariant detailsWindowVariant = DetailsWindowVariant.Building;
    public DetailsWindowVariant DetailsWindowVariant => detailsWindowVariant;

    [Header("Other")]
    [SerializeField] private bool instantConstruction = false;
    public bool InstantConstruction => instantConstruction;

    [SerializeField] private Sprite thumbImage = null;
    public Sprite ThumbImage => thumbImage;

    [SerializeField] private bool isDemolishable = true;
    public bool IsDemolishable => isDemolishable;

    [SerializeField] private bool isRaidable = false;
    public bool IsRaidable => isRaidable;
}