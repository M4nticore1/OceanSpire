using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour
{
    protected CityManager cityManager = null;
    public NavMeshAgent navMeshAgent { get; private set; } = null;

    [SerializeField] protected int maxHealth = 100;
    public int MaxHealth => maxHealth;
    protected int currentHealth = 100;
    [HideInInspector] public Building currentBuilding { get; protected set; } = null;
    [HideInInspector] public Building targetBuilding { get; protected set; } = null;
    public List<Building> pathBuildings = new List<Building>();
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public int currentFloorIndex { get; protected set; } = 0;
    [HideInInspector] public int currentBuildingPlaceIndex { get; protected set; } = 0;

    protected bool isWalking = false;
    public bool isRidingOnElevator { get; protected set; } = false;
    public bool isElevatorRiding { get; protected set; } = false;
    public bool isElevatorWaiting { get; protected set; } = false;
    public bool isElevatorWalking { get; protected set; } = false;

    public string firstName = "";
    public string lastName = "";

    protected virtual void Start()
    {
        cityManager = FindAnyObjectByType<CityManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {

    }

    public virtual void SetTargetBuilding(Building targetBuilding)
    {
        //if (currentBuilding)
        //    Debug.Log(currentBuilding.buildingData.buildingIdName);
        //else
        //    Debug.Log("!CurrentBuilding");

        this.targetBuilding = targetBuilding;
        pathIndex = 0;

        if (cityManager)
        {
            BuildingPlace startBuildingPlace = null;
            //BuildingPlace targetBuildingPlace = cityManager.spawnedFloors[targetBuilding.GetFloorIndex()].roomBuildingPlaces[targetBuilding.GetBuildingPlaceIndex()];

            if (currentBuilding)
                startBuildingPlace = cityManager.spawnedFloors[currentBuilding.GetFloorIndex()].roomBuildingPlaces[currentBuilding.GetBuildingPlaceIndex()];
            else
                startBuildingPlace = cityManager.spawnedFloors[CityManager.firstBuildCityFloorIndex].roomBuildingPlaces[CityManager.firstBuildCitybuildingPlace];

            bool isPathFounded = cityManager.FindPathToBuilding(startBuildingPlace, targetBuilding.buildingPlace, ref pathBuildings);

            if (isPathFounded)
            {
                FollowPath();
            }
        }
        else
            Debug.LogError("cityManager is NULL");
    }

    public void TakeDamage(int damange)
    {
        if (damange > 0)
        {
            currentHealth -= damange;

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
    }

    public virtual void EnterBuilding(Building building)
    {
        if (building)
        {
            //Debug.Log("Enter Building (entity)");

            currentBuilding = building;
            currentFloorIndex = building.GetFloorIndex();

            building.EnterBuilding(this);

            if (currentBuilding.GetFloorIndex() == pathBuildings[pathIndex].GetFloorIndex() && currentBuilding.GetBuildingPlaceIndex() == pathBuildings[pathIndex].GetBuildingPlaceIndex())
            {
                pathIndex++;

                FollowPath();
            }
        }
    }

    private void FollowPath()
    {
        if (!isElevatorRiding && pathIndex < pathBuildings.Count)
        {
            ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;
            Building nextBuilding = pathBuildings[pathIndex];
            ElevatorBuilding nextElevatorBuilding = nextBuilding as ElevatorBuilding;

            if (currentElevatorBuilding)
            {
                if (nextElevatorBuilding && nextElevatorBuilding.GetBuildingPlaceIndex() == currentElevatorBuilding.GetBuildingPlaceIndex() && nextElevatorBuilding.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
                {
                    //Debug.Log("First Elevator");
                    navMeshAgent.SetDestination(currentElevatorBuilding.spawnedElevatorPlatform.buildingInteractions[currentElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
                }
                else
                {
                    //Debug.Log("Second Elevator");

                    if (nextElevatorBuilding)
                    {
                        navMeshAgent.SetDestination(nextElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[nextElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
                    }
                    else if (nextBuilding)
                    {
                        navMeshAgent.SetDestination(nextBuilding.spawnedBuildingConstruction.transform.position);
                    }
                }
            }
            else
            {
                if (nextElevatorBuilding)
                {
                    if (nextElevatorBuilding.spawnedBuildingConstruction.buildingInteractions.Count > 0)
                    {
                        //Debug.Log("Moving to interaction point");
                        navMeshAgent.SetDestination(nextElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[nextElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].position);
                    }
                    else
                    {
                        Debug.LogWarning("buildingInteractions.Count is less");

                        navMeshAgent.SetDestination(nextElevatorBuilding.spawnedBuildingConstruction.transform.position);
                    }
                }
                else if (nextBuilding)
                {
                    navMeshAgent.SetDestination(nextBuilding.spawnedBuildingConstruction.transform.position);
                }
            }

            //if (!isElevatorRiding)
            //{
            //    //Debug.Log("FollowPath");

            //    if (currentElevatorBuilding && nextElevatorBuilding && nextElevatorBuilding.buildingPlace.buildingPlaceIndex == currentElevatorBuilding.buildingPlace.buildingPlaceIndex && nextElevatorBuilding.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
            //    {
            //        //Debug.Log("1 1");

            //        ElevatorPlatformConstruction elevatorPlatformConstruction = nextElevatorBuilding.spawnedElevatorPlatform;
            //        int ridingResidentIndex = elevatorPlatformConstruction.elevatorRidingPassengers.Count;

            //        int levelIndex = nextBuilding.levelIndex;

            //        if (elevatorPlatformConstruction.elevatorRidingPassengers.Count < nextBuilding.buildingLevelsData[levelIndex].maxResidentsCount)
            //        {
            //            if (navMeshAgent && navMeshAgent.enabled)
            //                navMeshAgent.SetDestination(elevatorPlatformConstruction.buildingInteractions[ridingResidentIndex].waypoints[0].position);
            //        }
            //        else
            //        {
            //            StartElevatorWaiting(currentElevatorBuilding);
            //        }

            //        //if (elevatorPlatformConstruction.currentFloorIndex == currentFloorIndex)
            //        //{
            //        //    currentElevatorBuilding.RemoveWalkingPassenger(this);
            //        //}
            //        //else
            //        //{
            //        //    currentElevatorBuilding.RemoveWalkingPassenger(this);
            //        //}
            //    }
            //    else
            //    {
            //        //Debug.Log("1 2");

            //        if (nextElevatorBuilding)
            //        {
            //            ElevatorPlatformConstruction elevatorPlatformConstruction = nextElevatorBuilding.spawnedElevatorPlatform;
            //            BuildingConstruction elevatorConstruction = nextBuilding.spawnedBuildingConstruction;

            //            //StartElevatorWalking(nextElevatorBuilding);

            //            int waitingResidentIndex = nextElevatorBuilding.elevatorWaitingPassengers.Count;

            //            if (elevatorConstruction.buildingInteractions.Count > waitingResidentIndex)
            //            {
            //                navMeshAgent.SetDestination(elevatorConstruction.buildingInteractions[waitingResidentIndex].waypoints[0].position);
            //            }
            //            else
            //            {
            //                navMeshAgent.SetDestination(elevatorPlatformConstruction.transform.position);
            //                Debug.LogWarning("buildingInteractions.Count < waitingResidentIndex");
            //            }
            //        }
            //        else
            //        {
            //            navMeshAgent.SetDestination(nextBuilding.spawnedBuildingConstruction.buildingInteractions[0].waypoints[0].position);
            //        }
            //    }
            //}

            //Debug.Log(pathBuildings[pathIndex].floorIndex + " " + pathBuildings[pathIndex].buildingPlace.buildingPlaceIndex);
        }
    }

    public virtual void ExitBuilding()
    {
        //currentBuilding = null;
    }

    public void SetFloorIndex(int newFloorIndex)
    {
        currentFloorIndex = newFloorIndex;
    }

    public void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed;
    }

    public void StartElevatorRiding(ElevatorBuilding elevatorBuilding)
    {
        //Debug.Log("Start Riding");

        isElevatorWaiting = false;
        isElevatorWalking = false;

        ElevatorPlatformConstruction elevatorPlatformConstruction = elevatorBuilding.spawnedElevatorPlatform;

        navMeshAgent.SetDestination(elevatorPlatformConstruction.buildingInteractions[elevatorPlatformConstruction.elevatorRidingPassengers.Count].waypoints[0].position);

        //FollowPath();

        elevatorBuilding.AddRidingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StopElevatorRiding(ElevatorBuilding elevatorBuilding)
    {
        isElevatorRiding = false;

        navMeshAgent.enabled = true;

        FollowPath();

        elevatorBuilding.RemoveRidingPassenger(this);
    }

    public void StartElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        //Debug.Log("Start Waiting");

        isElevatorRiding = false;
        isElevatorWaiting = true;
        isElevatorWalking = false;

        //BuildingConstruction buildingConstruction = elevatorBuilding.spawnedBuildingConstruction;

        //navMeshAgent.SetDestination(buildingConstruction.buildingInteractions[elevatorBuilding.elevatorWalkingPassengers.Count].waypoints[0].position);

        //FollowPath();

        elevatorBuilding.RemoveRidingPassenger(this);
        elevatorBuilding.AddWaitingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StopElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        isElevatorWaiting = false;

        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StartElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Walking");

        isElevatorRiding = false;
        isElevatorWaiting = false;
        isElevatorWalking = true;

        //BuildingConstruction buildingConstruction = elevatorBuilding.spawnedBuildingConstruction;

        //FollowPath();

        //navMeshAgent.SetDestination(buildingConstruction.buildingInteractions[elevatorBuilding.elevatorWalkingPassengers.Count].waypoints[0].position);

        elevatorBuilding.RemoveRidingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
        elevatorBuilding.AddWalkingPassenger(this);
    }

    public void StopElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        isElevatorWalking = false;

        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StartRidingOnElevator()
    {
        isElevatorRiding = true;

        navMeshAgent.enabled = false;
    }
}
