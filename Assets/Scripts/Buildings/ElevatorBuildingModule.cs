using System.Collections.Generic;
using UnityEngine;

public class ElevatorBuildingModule : BuildingModule
{
    public ElevatorPlatformConstruction spawnedElevatorCabin { get; private set; } = null;
    public int elevatorGroupId { get; private set; } = 0;

    public List<Creature> elevatorWaitingPassengers { get; private set; } = new List<Creature>();

    protected override void OnBuildingInited()
    {

    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding()
    {

    }

    protected override void OnExitBuilding()
    {

    }

    protected override void OnResidentStartWorking()
    {

    }

    protected override void OnResidentStopWorking()
    {

    }

    private void BuildConstruction()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        ElevatorBuildingModule belowElevatorBuilding = ownedTowerBuilding.downNeighborBuilding.GetComponent<ElevatorBuildingModule>();
        ElevatorBuildingModule aboveElevatorBuilding = ownedTowerBuilding.upNeighborBuilding.GetComponent<ElevatorBuildingModule>();

        if (belowElevatorBuilding && belowElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = belowElevatorBuilding.spawnedElevatorCabin;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = aboveElevatorBuilding.spawnedElevatorCabin;
        }
        else {
            ElevatorModuleLevelData elevatorBuildingLevelData = LevelData as ElevatorModuleLevelData;

            if (ownedTowerBuilding.buildingPosition == BuildingPosition.Straight)
                spawnedElevatorCabin = Instantiate(elevatorBuildingLevelData.ElevatorPlatformStraight);
            else
                spawnedElevatorCabin = Instantiate(elevatorBuildingLevelData.ElevatorPlatformCorner);

            spawnedElevatorCabin.transform.position = transform.position;
            spawnedElevatorCabin.transform.rotation = transform.rotation;

            spawnedElevatorCabin.Init(OwnedBuilding);

            elevatorGroupId = CityManager.Instance.elevatorGroups.Count;
        }
    }

    public void AddPassenger(Creature passenger)
    {
        spawnedElevatorCabin.AddPassenger(passenger);
    }

    public void RemovePassenger(Creature passenger)
    {
        spawnedElevatorCabin.RemovePassenger(passenger);
    }

    public bool IsPossibleToEnter()
    {
        return !spawnedElevatorCabin.isMoving && (spawnedElevatorCabin.OwnedElevator.OwnedBuilding as TowerBuilding).floorIndex == (OwnedBuilding as TowerBuilding).floorIndex && spawnedElevatorCabin.ridingPassengers.Count < OwnedBuilding.LevelData.maxResidentsCount;
    }

    public bool IsPossibleToExit()
    {
        return !spawnedElevatorCabin.isMoving;
    }

    public Transform GetCabinRidingTransform()
    {
        int ridersCount = spawnedElevatorCabin.ridingPassengers.Count;
        int goingToRidingCount = spawnedElevatorCabin.goingToRidingPassengers.Count;
        int index = ((ridersCount > 0 ? (ridersCount - 1) : 0) + (goingToRidingCount > 0 ? (goingToRidingCount - 1) : 0)) % spawnedElevatorCabin.BuildingInteractions.Length;
        return spawnedElevatorCabin.BuildingInteractions[index].waypoints[0];
    }
}
