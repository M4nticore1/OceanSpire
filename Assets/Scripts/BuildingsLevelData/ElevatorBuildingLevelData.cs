using UnityEngine;

[CreateAssetMenu(fileName = "ElevatorLevelData", menuName = "Scriptable Objects/ElevatorLevelData")]
public class ElevatorBuildingLevelData : RoomBuildingLevelData
{
    [Header("Elevator Constructions")]
    public ElevatorPlatformConstruction elevatorPlatformStraight;
    public ElevatorPlatformConstruction elevatorPlatformCorner;

    [Header("Elevator")]
    public float elevatorMoveSpeed = 0.0f;

    //public BuildingConstruction elevator;
    //public BuildingConstruction elevatorButtom;
    //public BuildingConstruction elevatorTop;
    //public BuildingConstruction elevatorTopButtom;
    //public BuildingConstruction elevatorPlatform;
}
