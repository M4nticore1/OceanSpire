using UnityEngine;

[CreateAssetMenu(fileName = "ElevatorLevelData", menuName = "Constructions Level Data/ElevatorLevelData")]
public class ElevatorLevelData : RoomLevelData
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
