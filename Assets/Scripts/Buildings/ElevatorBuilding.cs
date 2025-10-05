using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[AddComponentMenu("Buildings/ElevatorBuilding")]
public class ElevatorBuilding : RoomBuilding
{
    private bool hasElevatorPlatform = false;
    public ElevatorPlatformConstruction spawnedElevatorPlatform = null;
    public int elevatorGroupId = 0;

    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();

    protected override void Update()
    {
        base.Update();
    }

    public override void Build(BuildingPlace buildingPlace)
    {
        base.Build(buildingPlace);

        if (GetType() == typeof(ElevatorBuilding))
            InvokeBuildingPlaced(this);
    }

    protected override void UpdateBuildingConstruction()
    {
        base.UpdateBuildingConstruction();

        if (aboveConnectedBuilding && belowConnectedBuilding)
        {
            ElevatorBuilding upElevatorBuilding = aboveConnectedBuilding as ElevatorBuilding;
            ElevatorBuilding downElevatorBuilding = belowConnectedBuilding as ElevatorBuilding;

            if (downElevatorBuilding)
                spawnedElevatorPlatform = downElevatorBuilding.spawnedElevatorPlatform;

            elevatorGroupId = downElevatorBuilding.elevatorGroupId;
        }
        else if (aboveConnectedBuilding)
        {
            ElevatorBuilding elevatorBuilding = aboveConnectedBuilding as ElevatorBuilding;

            if (elevatorBuilding)
                spawnedElevatorPlatform = elevatorBuilding.spawnedElevatorPlatform;

            elevatorGroupId = elevatorBuilding.elevatorGroupId;
        }
        else if (belowConnectedBuilding)
        {
            ElevatorBuilding elevatorBuilding = belowConnectedBuilding as ElevatorBuilding;

            if (elevatorBuilding)
                spawnedElevatorPlatform = elevatorBuilding.spawnedElevatorPlatform;

            elevatorGroupId = elevatorBuilding.elevatorGroupId;
        }
        else
        {
            ElevatorBuildingLevelData elevatorBuildingLevelData = buildingLevelsData[levelIndex] as ElevatorBuildingLevelData;

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

        if (entity.pathBuildings.Count > 0 && entity.pathBuildings.Count > entity.pathIndex && entity.pathBuildings[entity.pathIndex] == this)
        {
            if (!entity.isRidingOnElevator)
            {
                if (spawnedElevatorPlatform.currentFloorIndex == GetFloorIndex())
                {
                    if (spawnedElevatorPlatform.isMoving || spawnedElevatorPlatform.elevatorRidingPassengers.Count == buildingLevelsData[levelIndex].maxResidentsCount)
                    {
                        entity.StartElevatorWaiting(this);
                    }
                    else
                    {
                        Debug.Log("StartElevatorWalking");
                        entity.StartElevatorWalking(this);
                    }
                }
                else
                {
                    entity.StartElevatorWaiting(this);
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
}
