using System.Collections.Generic;
using UnityEngine;

public class ElevatorModule : BuildingModule
{
    public ElevatorCabinConstruction spawnedElevatorCabin { get; private set; } = null;
    public int elevatorGroupId { get; private set; } = 0;

    protected override void OnInit()
    {
        if (!TryApplyCabin())
            CreateCabin();
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

    }

    private bool TryApplyCabin()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
        ElevatorModule belowElevatorBuilding = ownedTowerBuilding.downBuilding?.GetComponent<ElevatorModule>();
        ElevatorModule aboveElevatorBuilding = ownedTowerBuilding.upBuilding?.GetComponent<ElevatorModule>();

        if (belowElevatorBuilding && belowElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = belowElevatorBuilding.spawnedElevatorCabin;
            return true;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.spawnedElevatorCabin) {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = aboveElevatorBuilding.spawnedElevatorCabin;
            return true;
        }
        return false;
    }

    private void CreateCabin()
    {
        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;
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

    public void AddPassenger(EntityCityNavigator passenger)
    {
        spawnedElevatorCabin.AddPassenger(passenger);
    }

    public void RemovePassenger(EntityCityNavigator passenger)
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

        int length = spawnedElevatorCabin.BuildingInteractions.Length;
        if (length > 0) {
            int index = ((ridersCount > 0 ? (ridersCount - 1) : 0) + (goingToRidingCount > 0 ? (goingToRidingCount - 1) : 0)) % length;
            return spawnedElevatorCabin.BuildingInteractions[index].waypoints[0].transform;
        }
        else {
            return transform;
        }
    }
}
