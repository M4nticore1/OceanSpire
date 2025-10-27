using System;
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
    public bool isWaitingForElevator { get; protected set; } = false;
    public bool isWalkingToElevator { get; protected set; } = false;

    public string firstName = "";
    public string lastName = "";

    protected virtual void Awake()
    {
        cityManager = FindAnyObjectByType<CityManager>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected virtual void OnEnable()
    {
        Building.onAnyBuildingStartConstructing += OnBuildingStartConstructing;
    }

    protected virtual void OnDisable()
    {
        Building.onAnyBuildingStartConstructing -= OnBuildingStartConstructing;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public virtual void SetTargetBuilding(Func<Building, bool> targetBuildingCondition)
    {
        pathIndex = 0;

        if (cityManager)
        {
            BuildingPlace startBuildingPlace = null;

            if (currentBuilding)
                startBuildingPlace = cityManager.builtFloors[currentBuilding.GetFloorIndex()].roomBuildingPlaces[currentBuilding.GetPlaceIndex()];
            else
                startBuildingPlace = cityManager.builtFloors[CityManager.firstBuildCityFloorIndex].roomBuildingPlaces[CityManager.firstBuildCitybuildingPlace];

            bool isPathFounded = cityManager.FindPathToBuilding(startBuildingPlace, targetBuildingCondition, ref pathBuildings);

            if (isPathFounded)
            {
                targetBuilding = pathBuildings[pathBuildings.Count - 1];
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

            if (pathBuildings.Count > pathIndex && currentBuilding.GetFloorIndex() == pathBuildings[pathIndex].GetFloorIndex() && currentBuilding.GetPlaceIndex() == pathBuildings[pathIndex].GetPlaceIndex())
            {
                FollowPath();
                pathIndex++;
            }
        }
    }

    private void FollowPath()
    {
        //Debug.Log("FollowPath");

        if (!isRidingOnElevator && pathIndex < pathBuildings.Count)
        {
            ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;
            Building currentPathBuilding = pathBuildings[pathIndex];
            ElevatorBuilding currentPathElevatorBuilding = currentPathBuilding as ElevatorBuilding;

            if (currentElevatorBuilding)
            {
                //Debug.Log("currentElevatorBuilding");

                if (currentPathElevatorBuilding && currentPathElevatorBuilding.GetPlaceIndex() == currentElevatorBuilding.GetPlaceIndex() && currentPathElevatorBuilding.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
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

    // Elevators
    public void StartElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Walking");

        isWalkingToElevator = true;
        isWaitingForElevator = false;
        isRidingOnElevator = false;

        ElevatorPlatformConstruction elevatorPlatformConstruction = elevatorBuilding.spawnedElevatorPlatform;
        navMeshAgent.SetDestination(elevatorPlatformConstruction.buildingInteractions[elevatorPlatformConstruction.elevatorRidingPassengers.Count].waypoints[0].position);

        elevatorBuilding.AddWalkingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
        elevatorBuilding.RemoveRidingPassenger(this);
    }

    public void StopElevatorWalking(ElevatorBuilding elevatorBuilding)
    {
        isWalkingToElevator = false;

        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StartElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Waiting");

        isWalkingToElevator = false;
        isWaitingForElevator = true;
        isRidingOnElevator = false;

        BuildingConstruction buildingConstruction = elevatorBuilding.spawnedBuildingConstruction;
        navMeshAgent.SetDestination(buildingConstruction.buildingInteractions[elevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].position);

        elevatorBuilding.RemoveRidingPassenger(this);
        elevatorBuilding.AddWaitingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
    }

    public void StopElevatorWaiting(ElevatorBuilding elevatorBuilding)
    {
        isWaitingForElevator = false;

        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StartElevatorRiding(ElevatorBuilding elevatorBuilding)
    {
        Debug.Log("Start Riding");

        isRidingOnElevator = true;
        isWalkingToElevator = false;
        isWaitingForElevator = false;

        navMeshAgent.enabled = false;

        elevatorBuilding.AddRidingPassenger(this);
        elevatorBuilding.RemoveWalkingPassenger(this);
        elevatorBuilding.RemoveWaitingPassenger(this);
    }

    public void StopElevatorRiding(ElevatorBuilding elevatorBuilding)
    {
        isRidingOnElevator = false;

        navMeshAgent.enabled = true;

        FollowPath();

        elevatorBuilding.RemoveRidingPassenger(this);
    }

    // Buildings
    protected virtual void OnBuildingStartConstructing(Building building)
    {
        if (targetBuilding)
        {
            SetTargetBuilding(b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex());
        }
    }

    public void Select()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
    }

    public void Deselect()
    {
        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Default");
        }
    }
}
