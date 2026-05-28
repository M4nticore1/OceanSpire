using System;
using UnityEngine;

public enum BuildingIdEnum
{
    FloorFrame,
    TowerGate,
    Dock,
    BasicElevator,
    LivingRooms,
    CoalGenerator,
    Farm,
    Kitchen,
    ResourceStorage,
    FoodStorage,
    BatteryRoom,
    WeaponStorage,
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

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Id")]
    [SerializeField] private BuildingIdEnum buildingId = BuildingIdEnum.TowerGate;
    public int BuildingId { get { return (int)buildingId; } }

    [SerializeField] private LocalizationItem localizationItem = null;
    public LocalizationItem NameLocalizationItem => localizationItem;

    [Header("Enums")]
    [SerializeField] private BuildingType buildingType = BuildingType.Room;
    public BuildingType BuildingType => buildingType;

    [SerializeField] private BuildingCategory buildingCategory = BuildingCategory.Construction;
    public BuildingCategory BuildingCategory => buildingCategory;

    [SerializeField] private ConnectionType connectionType = ConnectionType.None;
    public ConnectionType ConnectionType => connectionType;

    [SerializeField] private BuildingStrategyEnum buildingStrategy = BuildingStrategyEnum.WorkBuilding;
    public BuildingStrategyEnum BuildingStrategy => buildingStrategy;

    [Header("Other")]
    [SerializeField] private Sprite thumbImage = null;
    public Sprite ThumbImage => thumbImage;

    [SerializeField] private bool isConstructable = false;
    public bool IsConstructable => isConstructable;

    [SerializeField] private bool isWorkable = false;
    public bool IsWorkable => isWorkable;

    [SerializeField] private bool isDemolishable = true;
    public bool IsDemolishable => isDemolishable;

    [SerializeField] private bool isRaidable = false;
    public bool IsRaidable => isRaidable;
}