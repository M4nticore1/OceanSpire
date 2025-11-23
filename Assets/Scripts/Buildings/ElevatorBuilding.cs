using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Buildings/ElevatorBuilding")]
public class ElevatorBuilding : RoomBuilding
{
    public ElevatorPlatformConstruction spawnedElevatorPlatform { get; private set; } = null;
    public int elevatorGroupId = 0;

    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();

    public override void BuildConstruction(int levelIndex)
    {
        base.BuildConstruction(levelIndex);

        ElevatorBuilding belowElevatorBuilding = belowConnectedBuilding as ElevatorBuilding;
        ElevatorBuilding aboveElevatorBuilding = aboveConnectedBuilding as ElevatorBuilding;

        if (belowElevatorBuilding && belowElevatorBuilding.spawnedElevatorPlatform)
        {
            elevatorGroupId = belowElevatorBuilding.elevatorGroupId;
            spawnedElevatorPlatform = belowElevatorBuilding.spawnedElevatorPlatform;
        }
        else if (aboveElevatorBuilding && aboveElevatorBuilding.spawnedElevatorPlatform)
        {
            elevatorGroupId = aboveElevatorBuilding.elevatorGroupId;
            spawnedElevatorPlatform = aboveElevatorBuilding.spawnedElevatorPlatform;
        }
        else
        {
            ElevatorLevelData elevatorBuildingLevelData = buildingLevelsData[levelIndex] as ElevatorLevelData;

            if (buildingPosition == BuildingPosition.Straight)
                spawnedElevatorPlatform = Instantiate(elevatorBuildingLevelData.elevatorPlatformStraight, cityManager.towerRoot);
            else
                spawnedElevatorPlatform = Instantiate(elevatorBuildingLevelData.elevatorPlatformCorner, cityManager.towerRoot);

            spawnedElevatorPlatform.transform.position = transform.position;
            spawnedElevatorPlatform.transform.rotation = transform.rotation;

            spawnedElevatorPlatform.Build();
            spawnedElevatorPlatform.SetElevatorBuilding(this);

            elevatorGroupId = cityManager.elevatorGroups.Count;
        }
    }

    public override void EnterBuilding(Entity entity)
    {
        base.EnterBuilding(entity);

        if (entity.pathBuildings.Count > 0 && entity.pathBuildings.Count > entity.pathIndex && entity.pathBuildings[entity.pathIndex] == this && entity.pathBuildings[entity.pathIndex + 1] as ElevatorBuilding)
        {
            if (!entity.isRidingOnElevator)
            {
                if (spawnedElevatorPlatform.currentFloorIndex == GetFloorIndex())
                {
                    if (spawnedElevatorPlatform.isMoving || spawnedElevatorPlatform.elevatorRidingPassengers.Count == buildingLevelsData[levelComponent.LevelIndex].maxResidentsCount)
                    {
                        entity.StartElevatorWaiting();
                    }
                    else
                    {
                        Debug.Log("StartElevatorWalking");
                        entity.StartElevatorWalking();
                    }
                }
                else
                {
                    entity.StartElevatorWaiting();
                }
            }
        }
    }

    public override void ExitBuilding(Entity entity)
    {
        //entity.EnterBuilding(this);
    }

    public void CallElevator(int startFloorIndex)
    {
        spawnedElevatorPlatform.StartMovingToFloor(startFloorIndex);
    }

    public void AddRidingPassenger(Entity entity)
    {
        spawnedElevatorPlatform.AddRidingPassenger(entity);
    }

    public void RemoveRidingPassenger(Entity entity)
    {
        spawnedElevatorPlatform.RemoveRidingPassenger(entity);
    }

    public void AddWaitingPassenger(Entity entity)
    {
        elevatorWaitingPassengers.Add(entity);
        spawnedElevatorPlatform.AddWaitingPassenger(entity);
    }

    public void RemoveWaitingPassenger(Entity entity)
    {
        elevatorWaitingPassengers.Remove(entity);
        spawnedElevatorPlatform.RemoveWaitingPassenger(entity);
    }

    public void AddWalkingPassenger(Entity entity)
    {
        elevatorWalkingPassengers.Add(entity);
        spawnedElevatorPlatform.AddWalkingPassenger(entity);
    }

    public void RemoveWalkingPassenger(Entity entity)
    {
        elevatorWalkingPassengers.Remove(entity);
        spawnedElevatorPlatform.RemoveWalkingPassenger(entity);
    }

    public bool IsPossibleToEnter()
    {
        return !spawnedElevatorPlatform.isMoving && spawnedElevatorPlatform.currentFloorIndex == GetFloorIndex() && spawnedElevatorPlatform.elevatorRidingPassengers.Count < buildingLevelsData[levelComponent.LevelIndex].maxResidentsCount;
    }

    public Vector3 GetPlatformRidingPosition()
    {
        int index = spawnedElevatorPlatform.elevatorRidingPassengers.Count % spawnedElevatorPlatform.buildingInteractions.Count;
        return spawnedElevatorPlatform.buildingInteractions[index].waypoints[0].position;
    }
}
