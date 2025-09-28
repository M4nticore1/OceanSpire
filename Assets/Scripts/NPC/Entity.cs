using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
                FollowPath();
                pathIndex++;
            }
        }
    }

    private void FollowPath()
    {
        //Debug.Log("FollowPath");

        if (!isElevatorRiding && pathIndex < pathBuildings.Count)
        {
            ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;
            Building currentPathBuilding = pathBuildings[pathIndex];
            ElevatorBuilding currentPathElevatorBuilding = currentPathBuilding as ElevatorBuilding;

            if (currentElevatorBuilding)
            {
                //Debug.Log("currentElevatorBuilding");

                if (currentPathElevatorBuilding && currentPathElevatorBuilding.GetBuildingPlaceIndex() == currentElevatorBuilding.GetBuildingPlaceIndex() && currentPathElevatorBuilding.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
                {
                    //navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[currentPathElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
                }
                else
                {
                    if (currentPathElevatorBuilding)
                    {
                        //Debug.Log("currentPathElevatorBuilding");
                        navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[currentPathElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
                    }
                    else if (currentPathBuilding)
                    {
                        //Debug.Log("currentPathBuilding");
                        navMeshAgent.SetDestination(currentPathBuilding.spawnedBuildingConstruction.transform.position);
                    }
                }
            }
            else
            {
                if (currentPathElevatorBuilding)
                {
                    if (currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions.Count > 0)
                    {
                        //Debug.Log("Moving to interaction point");
                        navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[currentPathElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].position);
                    }
                    else
                    {
                        Debug.LogWarning("buildingInteractions.Count is less");

                        navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.transform.position);
                    }
                }
                else if (currentPathBuilding)
                {
                    navMeshAgent.SetDestination(currentPathBuilding.spawnedBuildingConstruction.transform.position);
                }
            }

            //pathIndex++;
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

    public void StartElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Walking");

        isElevatorWalking = true;
        isElevatorWaiting = false;
        isElevatorRiding = false;

        ElevatorPlatformConstruction elevatorPlatformConstruction = elevatorBuilding.spawnedElevatorPlatform;
        navMeshAgent.SetDestination(elevatorPlatformConstruction.buildingInteractions[elevatorPlatformConstruction.elevatorRidingPassengers.Count].waypoints[0].position);

        Debug.Log(navMeshAgent.destination);

        elevatorBuilding.AddWalkingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
        elevatorBuilding.RemoveRidingPassenger(this);
    }

    public void StopElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        //isElevatorWalking = false;

        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StartElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Waiting");

        isElevatorWalking = false;
        isElevatorWaiting = true;
        isElevatorRiding = false;

        BuildingConstruction buildingConstruction = elevatorBuilding.spawnedBuildingConstruction;
        navMeshAgent.SetDestination(buildingConstruction.buildingInteractions[elevatorBuilding.elevatorWalkingPassengers.Count].waypoints[0].position);

        elevatorBuilding.RemoveRidingPassenger(this);
        elevatorBuilding.AddWaitingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StopElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        isElevatorWaiting = false;

        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StartElevatorRiding(ElevatorBuilding elevatorBuilding)
    {
        //Debug.Log("Start Riding");

        isElevatorWalking = false;
        isElevatorWaiting = false;
        isElevatorRiding = true;

        navMeshAgent.enabled = false;

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
}
