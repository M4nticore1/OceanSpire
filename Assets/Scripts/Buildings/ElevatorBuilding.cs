using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/ElevatorBuilding")]
public class ElevatorBuilding : RoomBuilding
{
    public ElevatorPlatformConstruction elevatorPlatform { get; private set; } = null;
    public int elevatorGroupId = 0;

    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    //public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();

    public override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        ElevatorBuilding belowElevatorBuilding = belowConnectedBuilding as ElevatorBuilding;
        ElevatorBuilding aboveElevatorBuilding = aboveConnectedBuilding as ElevatorBuilding;

        if (belowElevatorBuilding && belowElevatorBuilding.elevatorPlatform)
        {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            elevatorPlatform = belowElevatorBuilding.elevatorPlatform;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.elevatorPlatform)
        {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            elevatorPlatform = aboveElevatorBuilding.elevatorPlatform;
        }
        else
        {
            ElevatorLevelData elevatorBuildingLevelData = buildingLevelsData[levelIndex] as ElevatorLevelData;

            if (buildingPosition == BuildingPosition.Straight)
                elevatorPlatform = Instantiate(elevatorBuildingLevelData.ElevatorPlatformStraight, cityManager.towerRoot);
            else
                elevatorPlatform = Instantiate(elevatorBuildingLevelData.ElevatorPlatformCorner, cityManager.towerRoot);

            elevatorPlatform.transform.position = transform.position;
            elevatorPlatform.transform.rotation = transform.rotation;

            elevatorPlatform.Build(this);

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
        //            if (spawnedElevatorPlatform.isMoving || spawnedElevatorPlatform.elevatorRidingPassengers.Count == buildingLevelsData[levelComponent.LevelIndex].maxResidentsCount)
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

    public void AddRidingPassenger(Entity entity)
    {
        elevatorPlatform.AddRidingPassenger(entity);
    }

    public void RemoveRidingPassenger(Entity entity)
    {
        elevatorPlatform.RemoveRidingPassenger(entity);
    }

    public void AddWaitingPassenger(Entity entity)
    {
        elevatorWaitingPassengers.Add(entity);
        elevatorPlatform.AddWaitingPassenger(entity);
    }

    public void RemoveWaitingPassenger(Entity entity)
    {
        elevatorWaitingPassengers.Remove(entity);
        elevatorPlatform.RemoveWaitingPassenger(entity);
    }

    public bool IsPossibleToEnter()
    {
        return !elevatorPlatform.isMoving && elevatorPlatform.floorIndex == floorIndex && elevatorPlatform.ridingPassengers.Count < buildingLevelsData[levelComponent.LevelIndex].maxResidentsCount;
    }

    public Vector3 GetPlatformRidingPosition()
    {
        int index = (elevatorPlatform.ridingPassengers.Count - 1) % elevatorPlatform.BuildingInteractions.Count;
        return elevatorPlatform.BuildingInteractions[index].waypoints[0].position;
    }
}
