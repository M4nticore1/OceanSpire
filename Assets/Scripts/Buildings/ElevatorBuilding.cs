using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/ElevatorBuilding")]
public class ElevatorBuilding : RoomBuilding
{
    public ElevatorPlatformConstruction spawnedElevatorCabin { get; private set; } = null;
    public int elevatorGroupId = 0;

    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    //public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();

    public override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        ElevatorBuilding belowElevatorBuilding = downConnectedBuilding as ElevatorBuilding;
        ElevatorBuilding aboveElevatorBuilding = upConnectedBuilding as ElevatorBuilding;

        if (belowElevatorBuilding && belowElevatorBuilding.spawnedElevatorCabin)
        {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = belowElevatorBuilding.spawnedElevatorCabin;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.spawnedElevatorCabin)
        {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            spawnedElevatorCabin = aboveElevatorBuilding.spawnedElevatorCabin;
        }
        else
        {
            ElevatorLevelData elevatorBuildingLevelData = buildingLevelsData[levelIndex] as ElevatorLevelData;

            if (buildingPosition == BuildingPosition.Straight)
                spawnedElevatorCabin = Instantiate(elevatorBuildingLevelData.ElevatorPlatformStraight, cityManager.towerRoot);
            else
                spawnedElevatorCabin = Instantiate(elevatorBuildingLevelData.ElevatorPlatformCorner, cityManager.towerRoot);

            spawnedElevatorCabin.transform.position = transform.position;
            spawnedElevatorCabin.transform.rotation = transform.rotation;

            spawnedElevatorCabin.Build(this);

            elevatorGroupId = cityManager.elevatorGroups.Count;
        }
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);

        //if (entity.pathBuildings.Count > entity.pathIndex && entity.pathBuildings[entity.pathIndex + 1] as ElevatorBuilding)
        //{
        //    if (!entity.isRidingOnElevator)
        //    {
        //        if (spawnedElevatorPlatform.currentFloorIndex == floorIndex)
        //        {
        //            if (spawnedElevatorPlatform.isMoving || spawnedElevatorPlatform.elevatorRidingPassengers.Count == buildingLevelsData[levelIndex].maxResidentsCount)
        //            {
        //                Debug.Log("StartElevatorWaiting");
        //                entity.StartWaitingForElevator();
        //            }
        //            else
        //            {
        //                Debug.Log("StartElevatorWalking");
        //                entity.StartWalkingToElevator();
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("StartElevatorWaiting");
        //            entity.StartWaitingForElevator();
        //        }
        //    }
        //}
    }

    public override void ExitBuilding(Entity entity)
    {
        //entity.EnterBuilding(this);
    }

    public void AddPassenger(Entity passenger)
    {
        spawnedElevatorCabin.AddPassenger(passenger);
    }

    public void RemovePassenger(Entity passenger)
    {
        spawnedElevatorCabin.RemovePassenger(passenger);
    }

    public bool IsPossibleToEnter()
    {
        return !spawnedElevatorCabin.isMoving && spawnedElevatorCabin.floorIndex == floorIndex && spawnedElevatorCabin.ridingPassengers.Count < currentLevelData.maxResidentsCount;
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
