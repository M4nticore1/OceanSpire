using UnityEngine;

[CreateAssetMenu(fileName = "ElevatorLevelData", menuName = "Constructions Level Data/ElevatorLevelData")]
public class ElevatorLevelData : RoomLevelData
{
    [Header("Elevator Constructions")]
    [SerializeField] private ElevatorPlatformConstruction elevatorPlatformStraight;
    public ElevatorPlatformConstruction ElevatorPlatformStraight => elevatorPlatformStraight;
    [SerializeField] private ElevatorPlatformConstruction elevatorPlatformCorner;
    public ElevatorPlatformConstruction ElevatorPlatformCorner => elevatorPlatformCorner;

    [Header("Elevator")]
    [SerializeField] private float elevatorMoveSpeed = 0.0f;
    public float ElevatorMoveSpeed => elevatorMoveSpeed;

    //public BuildingConstruction elevator;
    //public BuildingConstruction elevatorButtom;
    //public BuildingConstruction elevatorTop;
    //public BuildingConstruction elevatorTopButtom;
    //public BuildingConstruction elevatorPlatform;
}
