using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ElevatorPlatformConstruction : BuildingConstruction
{
    private CityManager cityManager = null;

    public List<Entity> elevatorWaitingPassengers { get; private set; } = new List<Entity>();
    //public List<Entity> elevatorWalkingPassengers { get; private set; } = new List<Entity>();
    public List<Entity> elevatorRidingPassengers { get; private set; } = new List<Entity>();

    public bool isMoving { get; private set; } = false;
    public int currentFloorIndex { get; private set; } = 0;
    public int startFloorIndex { get; private set; } = 0;
    public int targetFloorIndex { get; private set; } = 0;

    private float moveSpeed = 0.0f;
    private Vector3 moveDirection = Vector3.zero;

    private ElevatorBuilding elevatorBuilding = null;

    protected void Update()
    {
        if (isMoving)
        {
            int floorIndex = 0;

            if (targetFloorIndex > currentFloorIndex)
            {
                floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight) / CityManager.floorHeight);

                if(floorIndex < startFloorIndex)
                    floorIndex = startFloorIndex;
            }
            else
            {
                floorIndex = (int)((transform.position.y - CityManager.firstFloorHeight + CityManager.floorHeight) / CityManager.floorHeight);

                if (floorIndex > startFloorIndex)
                    floorIndex = startFloorIndex;
            }

            SetFloorIndex(floorIndex);

            if (floorIndex != targetFloorIndex)
            {
                float speed = moveSpeed * Time.deltaTime;
                Move(moveDirection, speed);
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            if (elevatorWaitingPassengers.Count > 0 || elevatorRidingPassengers.Count > 0)
            {
                bool canMove = false;

                if (elevatorRidingPassengers.Count == elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount || (elevatorBuilding.elevatorWaitingPassengers.Count == 0 && elevatorBuilding.elevatorWalkingPassengers.Count == 0))
                {
                    //Debug.Log("2");

                    for (int i = 0; i < elevatorRidingPassengers.Count; i++)
                    {
                        float distance = math.distance(elevatorRidingPassengers[i].transform.position, buildingInteractions[i].waypoints[0].position);

                        if (distance < 1.0f && elevatorRidingPassengers[i].navMeshAgent.velocity == Vector3.zero)
                        {
                            canMove = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (elevatorBuilding.elevatorWaitingPassengers.Count < elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount)
                {
                    int waitingPassengersCount = elevatorBuilding.elevatorWaitingPassengers.Count;

                    waitingPassengersCount = math.clamp(waitingPassengersCount, 0, elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount);

                    for (int i = 0; i < waitingPassengersCount; i++)
                    {
                        float distance = math.distance(elevatorWaitingPassengers[i].transform.position, buildingInteractions[i].waypoints[0].position);

                        if (distance < 1.0f && elevatorWaitingPassengers[i].navMeshAgent.velocity == Vector3.zero)
                        {
                            canMove = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (canMove)
                {
                    StartMovingToFloor(GetNextFloor());
                }
            }
        }

        //Debug.Log(currentFloorIndex);
    }

    public override void Build()
    {
        base.Build();
    }

    public void StartMovingToFloor(int targetFloorIndex)
    {
        if (targetFloorIndex != currentFloorIndex)
        {
            //Debug.Log("StartMoving");

            isMoving = true;
            startFloorIndex = currentFloorIndex;

            if (elevatorRidingPassengers.Count > 0)
                this.targetFloorIndex = elevatorRidingPassengers[0].targetBuilding.GetFloorIndex();
            else
                this.targetFloorIndex = targetFloorIndex;

            if (this.targetFloorIndex > currentFloorIndex)
                moveDirection = Vector3.up;
            else if (this.targetFloorIndex < currentFloorIndex)
                moveDirection = Vector3.down;

            for (int i = 0; i < elevatorRidingPassengers.Count; i++)
            {
                elevatorRidingPassengers[i].StartRidingOnElevator(); // Disable nav agent
            }
        }
    }

    public void StopMoving()
    {
        Debug.Log("StopMoving");

        isMoving = false;
        transform.position = new Vector3(transform.position.x, currentFloorIndex * CityManager.floorHeight + CityManager.firstFloorHeight, transform.position.z);

        bool isNeededToMove = false;

        for (int i = 0; i < elevatorRidingPassengers.Count; i++)
        {
            if (elevatorRidingPassengers[i].pathBuildings[elevatorRidingPassengers[i].pathIndex].GetFloorIndex() == currentFloorIndex)
            {
                //Debug.Log("Path");
                elevatorRidingPassengers[i].StopElevatorRiding(elevatorBuilding);
            }
            else
            {
                //Debug.Log("!Path");
                isNeededToMove = true;
            }
        }

        int newRidersCount = elevatorBuilding.elevatorWaitingPassengers.Count;

        newRidersCount = math.clamp(newRidersCount, 0, elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount - elevatorRidingPassengers.Count);

        for (int i = 0; i < newRidersCount; i++)
        {
            elevatorBuilding.elevatorWaitingPassengers[i].StartElevatorRiding(elevatorBuilding);
        }

        if (elevatorWaitingPassengers.Count > 0 && newRidersCount == 0)
        {
            isNeededToMove = true;
        }

        if (isNeededToMove)
            StartMovingToFloor(GetNextFloor());

        //StartMovingToFloor(GetNextFloor());
    }

    private int GetNextFloor()
    {
        int nearestFloorIndex = 0;

        if (elevatorRidingPassengers.Count > 0)
            this.targetFloorIndex = elevatorRidingPassengers[0].targetBuilding.GetFloorIndex();
        else
            this.targetFloorIndex = targetFloorIndex;

        if (elevatorRidingPassengers.Count > 0 /*elevatorWaitingPassengers.Count > 0*/ /*elevatorRidingPassengers.Count < elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex].maxResidentsCount*/)
        {
            Debug.Log("elevatorRidingPassengers.Count > 0");

            if (targetFloorIndex > currentFloorIndex)
            {
                nearestFloorIndex = elevatorBuilding.cityManager.builtFloorsCount - 1;

                for (int i = 0; i < elevatorWaitingPassengers.Count; i++)
                {
                    if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < targetFloorIndex && elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() > currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() <= targetFloorIndex)
                    {
                        if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < nearestFloorIndex)
                        {
                            nearestFloorIndex = elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex();
                        }
                    }
                }
            }
            else if (targetFloorIndex < currentFloorIndex)
            {
                nearestFloorIndex = 0;

                for (int i = 0; i < elevatorWaitingPassengers.Count; i++)
                {
                    if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > targetFloorIndex && elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() < currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() < currentFloorIndex && elevatorWaitingPassengers[i].targetBuilding.GetFloorIndex() >= targetFloorIndex)
                    {
                        if (elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex() > nearestFloorIndex)
                        {
                            nearestFloorIndex = elevatorWaitingPassengers[i].currentBuilding.GetFloorIndex();
                        }
                    }
                }
            }
            else
            {
                nearestFloorIndex = targetFloorIndex;
            }
        }
        else if (elevatorWaitingPassengers.Count > 0)
        {
            Debug.Log(elevatorWaitingPassengers.Count);

            nearestFloorIndex = elevatorWaitingPassengers[0].currentFloorIndex;
        }
        else
        {
            nearestFloorIndex = targetFloorIndex;
        }

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

    public void AddWaitingPassenger(Entity passenger)
    {
        elevatorWaitingPassengers.Add(passenger);

        Debug.Log(passenger.currentFloorIndex);

        StartMovingToFloor(GetNextFloor());
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
        if (newFloorIndex != currentFloorIndex)
        {
            currentFloorIndex = newFloorIndex;
            elevatorBuilding = cityManager.spawnedFloors[newFloorIndex].roomBuildingPlaces[elevatorBuilding.GetBuildingPlaceIndex()].placedBuilding as ElevatorBuilding;

            for (int i = 0; i < elevatorRidingPassengers.Count; i++)
            {
                elevatorRidingPassengers[i].EnterBuilding(elevatorBuilding);
            }
        }
    }

    public void SetElevatorBuilding(ElevatorBuilding elevatorBuilding)
    {
        ElevatorBuildingLevelData elevatorBuildingLevelData = elevatorBuilding.buildingLevelsData[elevatorBuilding.levelIndex] as ElevatorBuildingLevelData;

        this.elevatorBuilding = elevatorBuilding;
        cityManager = elevatorBuilding.cityManager;
        moveSpeed = elevatorBuildingLevelData.elevatorMoveSpeed;

        SetFloorIndex(elevatorBuilding.GetFloorIndex());
        
    }
}
