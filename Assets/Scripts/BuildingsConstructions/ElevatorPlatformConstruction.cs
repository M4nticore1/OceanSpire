using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ElevatorPlatformConstruction : BuildingConstruction
{
    private CityManager cityManager = null;

    public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> elevatorRidingPassengers { get; private set; } = new List<Entity>();

    public bool isMoving { get; private set; } = false;
    public int currentFloorIndex = 0;
    public int startFloorIndex { get; private set; } = 0;
    public int currentTargetFloorIndex { get; private set; } = 0;
    public int placeIndex { get; private set; } = 0;

    private float moveSpeed = 0.0f;
    private Vector3 moveDirection = Vector3.zero;

    private ElevatorBuilding elevatorBuilding = null;

    protected void Update()
    {
        if (isMoving)
        {
            SetFloorIndex(GetFloorIndexByPosition(currentTargetFloorIndex));

            float speed = moveSpeed * Time.deltaTime;
            Move(moveDirection, speed);

            if (currentFloorIndex == currentTargetFloorIndex)
            {
                StopMoving();
            }
        }
        else
        {
            if (elevatorWalkingPassengers.Count > 0)
            {
                bool canMove = true;

                List<Entity> currentElevatorWalkingPassengers = elevatorWalkingPassengers;
                for (int i = 0; i < currentElevatorWalkingPassengers.Count; i++)
                {
                    float distance = 0;

                    if (buildingInteractions.Count > 0)
                        distance = math.distance(currentElevatorWalkingPassengers[i].transform.position, currentElevatorWalkingPassengers[i].targetPosition);

                    if (distance <= 1f && currentElevatorWalkingPassengers[i].navMeshAgent.velocity == Vector3.zero)
                    {
                        currentElevatorWalkingPassengers[i].StartElevatorRiding();
                    }
                    else
                    {
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    StartMovingToFloor(GetNextFloor());
                }
            }
            else if (elevatorRidingPassengers.Count > 0)
            {
                StartMovingToFloor(GetNextFloor());
            }
            else if (elevatorWaitingPassengers.Count > 0)
            {

                bool canMove = true;

                float distance = math.distance(elevatorWaitingPassengers[0].transform.position, elevatorWaitingPassengers[0].currentBuilding.spawnedBuildingConstruction.buildingInteractions[0].waypoints[0].position);

                if (distance > 1f || elevatorWaitingPassengers[0].navMeshAgent.velocity != Vector3.zero)
                {
                    canMove = false;
                }

                if (canMove)
                {
                    StartMovingToFloor(GetNextFloor());
                }
            }
        }
    }

    public override void Build()
    {
        base.Build();

        cityManager = FindAnyObjectByType<CityManager>();
    }

    public void StartMovingToFloor(int targetFloorIndex)
    {
        if (targetFloorIndex != currentFloorIndex)
        {
            Debug.Log(elevatorRidingPassengers.Count);

            isMoving = true;
            startFloorIndex = currentFloorIndex;

            if (currentTargetFloorIndex > currentFloorIndex)
                moveDirection = Vector3.up;
            else if (currentTargetFloorIndex < currentFloorIndex)
                moveDirection = Vector3.down;
        }
    }

    public void StopMoving()
    {
        isMoving = false;

        // Correct position
        transform.position = new Vector3(transform.position.x, currentFloorIndex * CityManager.floorHeight + CityManager.firstFloorHeight, transform.position.z);

        // Stop entities riding
        for (int i = elevatorRidingPassengers.Count - 1; i >= 0; i--)
        {
            var rider = elevatorRidingPassengers[i];
            if (rider.pathBuildings.Count > rider.pathIndex + 1 && rider.pathBuildings[rider.pathIndex + 1].GetFloorIndex() == currentFloorIndex)
            {
                rider.StopElevatorRiding();
            }
        }

        int newRidersCount = elevatorBuilding.elevatorWaitingPassengers.Count;

        newRidersCount = math.clamp(newRidersCount, 0, elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount - elevatorRidingPassengers.Count);

        Debug.Log(newRidersCount);
        for (int i = 0; i < newRidersCount; i++)
        {
            elevatorBuilding.elevatorWaitingPassengers[i].StartElevatorWalking();
        }
    }

    private int GetNextFloor()
    {
        int nearestFloorIndex = currentFloorIndex;

        if (elevatorRidingPassengers.Count > 0)
        {
            if (true || elevatorRidingPassengers.Count == elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount)
            {
                int currentTargetFloorIndex = elevatorRidingPassengers[0].pathBuildings[elevatorRidingPassengers[0].pathIndex].GetFloorIndex();

                if (currentTargetFloorIndex > currentFloorIndex)
                {
                    nearestFloorIndex = elevatorBuilding.cityManager.builtFloors.Count - 1;

                    for (int i = 0; i < elevatorRidingPassengers.Count; i++)
                    {
                        int nextFloorIndex = elevatorRidingPassengers[i].pathBuildings[elevatorRidingPassengers[i].pathIndex].GetFloorIndex();

                        if (nextFloorIndex < nearestFloorIndex)
                            nearestFloorIndex = nextFloorIndex;
                    }
                }
                else
                {
                    nearestFloorIndex = 0;

                    for (int i = 0; i < elevatorRidingPassengers.Count; i++)
                    {
                        int nextFloorIndex = elevatorRidingPassengers[i].pathBuildings[elevatorRidingPassengers[i].pathIndex].GetFloorIndex();

                        if (nextFloorIndex > nearestFloorIndex)
                            nearestFloorIndex = nextFloorIndex;
                    }
                }
            }
            else
            {
                if (elevatorWaitingPassengers.Count > 0)
                {
                    int currentTargetFloorIndex = elevatorRidingPassengers[0].pathBuildings[elevatorRidingPassengers[0].pathIndex].GetFloorIndex();

                    if (currentTargetFloorIndex > currentFloorIndex)
                    {
                        nearestFloorIndex = elevatorBuilding.cityManager.builtFloors.Count - 1;

                        for (int i = 0; i < elevatorWaitingPassengers.Count; i++)
                        {
                            if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < currentTargetFloorIndex && elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() > currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() <= currentTargetFloorIndex)
                            {
                                if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < nearestFloorIndex)
                                {
                                    nearestFloorIndex = elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex();

                                    if (nearestFloorIndex == currentFloorIndex + 1)
                                        break;
                                }
                            }
                        }
                    }
                    else if (currentTargetFloorIndex < currentFloorIndex)
                    {
                        nearestFloorIndex = 0;

                        for (int i = 0; i < elevatorWaitingPassengers.Count; i++)
                        {
                            if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > currentTargetFloorIndex && elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() < currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() >= currentTargetFloorIndex)
                            {
                                if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > nearestFloorIndex)
                                {
                                    nearestFloorIndex = elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex();

                                    if (nearestFloorIndex == currentFloorIndex - 1)
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        nearestFloorIndex = currentTargetFloorIndex;
                    }
                }
            }
        }
        else if (elevatorWaitingPassengers.Count > 0)
        {
            nearestFloorIndex = elevatorWaitingPassengers[0].currentFloorIndex;
        }

        currentTargetFloorIndex = nearestFloorIndex;
        return nearestFloorIndex;
    }

    private void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;

        for (int i = 0; i < elevatorRidingPassengers.Count; i++)
        {
            elevatorRidingPassengers[i].Move(direction, speed);
        }
    }

    public void AddWalkingPassenger(Entity passenger)
    {
        elevatorWalkingPassengers.Add(passenger);
    }

    public void RemoveWalkingPassenger(Entity passenger)
    {
        elevatorWalkingPassengers.Remove(passenger);
    }

    public void AddWaitingPassenger(Entity passenger)
    {
        elevatorWaitingPassengers.Add(passenger);
    }

    public void RemoveWaitingPassenger(Entity passenger)
    {
        elevatorWaitingPassengers.Remove(passenger);
    }

    public void AddRidingPassenger(Entity passenger)
    {
        elevatorRidingPassengers.Add(passenger);
    }

    public void RemoveRidingPassenger(Entity passenger)
    {
        elevatorRidingPassengers.Remove(passenger);
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public void SetFloorIndex(int newFloorIndex)
    {
        if (newFloorIndex != currentFloorIndex && newFloorIndex >= 0)
        {
            currentFloorIndex = newFloorIndex;

            elevatorBuilding = cityManager.builtFloors[newFloorIndex].roomBuildingPlaces[placeIndex].placedBuilding as ElevatorBuilding;

            for (int i = 0; i < elevatorRidingPassengers.Count; i++)
            {
                elevatorRidingPassengers[i].EnterBuilding(elevatorBuilding);
            }
        }
    }

    public int GetFloorIndexByPosition(int targetFloor)
    {
        int floorIndex = 0;

        if (targetFloor >= currentFloorIndex)
        {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight) / CityManager.floorHeight);

            if (floorIndex < startFloorIndex)
                floorIndex = startFloorIndex;
        }
        else
        {
            floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight + CityManager.floorHeight) / CityManager.floorHeight);

            if (floorIndex > startFloorIndex)
                floorIndex = startFloorIndex;
        }

        return floorIndex;
    }

    public void SetElevatorBuilding(ElevatorBuilding elevatorBuilding)
    {
        if (elevatorBuilding)
        {
            ElevatorBuildingLevelData elevatorBuildingLevelData = elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex] as ElevatorBuildingLevelData;

            this.elevatorBuilding = elevatorBuilding;
            cityManager = elevatorBuilding.cityManager;
            moveSpeed = elevatorBuildingLevelData.elevatorMoveSpeed;

            SetFloorIndex(elevatorBuilding.GetFloorIndex());
            placeIndex = elevatorBuilding.GetPlaceIndex();
        }
    }
}
