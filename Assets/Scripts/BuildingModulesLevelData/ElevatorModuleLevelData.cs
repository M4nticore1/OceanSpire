using UnityEngine;

[CreateAssetMenu(fileName = "ElevatorLevelData", menuName = "Modules Level Data/ElevatorLevelData")]
public class ElevatorModuleLevelData : BuildingModuleLevelData
{
    [Header("Elevator Constructions")]
    [SerializeField] private ElevatorCabinConstruction elevatorPlatformStraight;
    public ElevatorCabinConstruction ElevatorPlatformStraight => elevatorPlatformStraight;
    [SerializeField] private ElevatorCabinConstruction elevatorPlatformCorner;
    public ElevatorCabinConstruction ElevatorPlatformCorner => elevatorPlatformCorner;

    [Header("Elevator")]
    [SerializeField] private float elevatorMoveSpeed = 0.0f;
    public float ElevatorMoveSpeed => elevatorMoveSpeed;

    //public BuildingConstruction elevator;
    //public BuildingConstruction elevatorButtom;
    //public BuildingConstruction elevatorTop;
    //public BuildingConstruction elevatorTopButtom;
    //public BuildingConstruction elevatorPlatform;
}
