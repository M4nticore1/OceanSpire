using System;
using UnityEngine;

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
    Residental,
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
    public string buildingIdName = "";
    public string buildingName = "";
    public BuildingType buildingType = BuildingType.Room;
    public BuildingCategory buildingCategory = BuildingCategory.Construction;
    public ConnectionType connectionType = ConnectionType.None;

    public int buildingFloors = 1;
    public int maxBuildingFloors = 1;

    public bool isDemolishable = true;

    [Header("UI")]
    public GameObject buildingManagementMenuWidget = null;
}
