using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

public enum ResidentWork
{
    None,
    BuildingWork,
    ConstructingBuilding,
}

public class Entity : MonoBehaviour
{
    protected CityManager cityManager = null;
    public NavMeshAgent navMeshAgent { get; private set; } = null;

    [SerializeField] protected int maxHealth = 100;
    public int MaxHealth => maxHealth;
    protected int currentHealth = 100;

    public Building currentBuilding = null;
    [HideInInspector] public Building targetBuilding { get; protected set; } = null;
    public List<Building> pathBuildings = new List<Building>();
    [HideInInspector] public int pathIndex = 0;
    [HideInInspector] public int currentFloorIndex { get; protected set; } = 0;
    [HideInInspector] public int currentBuildingPlaceIndex { get; protected set; } = 0;
    protected Vector3 targetPosition = Vector3.zero;
    protected const float applyTargetPosition = 1.0f;

    protected bool isWalking = false;
    public bool isRidingOnElevator { get; protected set; } = false;
    public bool isWaitingForElevator { get; protected set; } = false;
    public bool isWalkingToElevator { get; protected set; } = false;

    [HideInInspector] public ResidentWork currentWork { get; protected set; } = ResidentWork.None;
    [HideInInspector] public bool isWorking { get; protected set; } = false;
    [HideInInspector] public int workerIndex { get; protected set; } = 0;
    [HideInInspector] public Building workBuilding { get; protected set; } = null;

    protected float actionTime = 0.0f;
    protected int actionIndex = 0;

    public string firstName = "";
    public string lastName = "";

    public static string[] possibleFirstNames = { "", };
    public static string[] possibleLastNames = { "", };

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
        Debug.Log(currentBuilding);
    }

    public Building SetTargetBuilding(BuildingPlace startBuildingPlace, Func<Building, bool> targetBuildingCondition)
    {
        pathIndex = 0;

        if (cityManager)
        {
            targetBuilding = cityManager.FindPathToBuilding(startBuildingPlace, targetBuildingCondition, ref pathBuildings);

            if (targetBuilding)
            {
                if (startBuildingPlace == (currentBuilding ? currentBuilding.buildingPlace : null))
                {
                    FollowPath();
                    return targetBuilding;
                }
                else
                {
                    if (cityManager.FindPathToBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex(), ref pathBuildings))
                    {
                        FollowPath();
                        return targetBuilding;
                    }
                }
            }
        }

        Debug.LogError("cityManager is NULL");
        return null;
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
            currentBuilding = building;
            currentFloorIndex = building.GetFloorIndex();

            building.EnterBuilding(this);

            if (pathBuildings.Count > pathIndex && currentBuilding.GetFloorIndex() == pathBuildings[pathIndex].GetFloorIndex() && currentBuilding.GetPlaceIndex() == pathBuildings[pathIndex].GetPlaceIndex())
            {
                FollowPath();
                pathIndex++;
            }
        }
        else
            Debug.LogWarning("building is NULL");
    }

    private void FollowPath()
    {
        if (!isRidingOnElevator && pathIndex < pathBuildings.Count)
        {
            ElevatorBuilding currentElevatorBuilding = currentBuilding as ElevatorBuilding;
            Building nextBuilding = pathBuildings[pathIndex];
            ElevatorBuilding nextElevatorBuilding = nextBuilding as ElevatorBuilding;

            if (nextBuilding)
            {
                if (nextBuilding == targetBuilding)
                {
                    if (currentWork != ResidentWork.None)
                    {
                        if (currentWork == ResidentWork.BuildingWork)
                        {
                            targetPosition = nextBuilding.GetInteractionPointPosition();
                        }
                        else if (currentWork == ResidentWork.ConstructingBuilding)
                        {
                            targetPosition = nextBuilding.GetPickupItemPointPosition();
                        }

                        navMeshAgent.SetDestination(targetPosition);
                    }
                }
                else if (nextElevatorBuilding)
                {
                    navMeshAgent.SetDestination(nextElevatorBuilding.GetInteractionPointPosition());
                }
            }
            else
            {
                if (currentElevatorBuilding)
                {
                    if (nextElevatorBuilding && nextElevatorBuilding.GetPlaceIndex() == currentElevatorBuilding.GetPlaceIndex() && nextElevatorBuilding.buildingData.buildingIdName == currentElevatorBuilding.buildingData.buildingIdName)
                    {
                        //navMeshAgent.SetDestination(currentPathElevatorBuilding.spawnedBuildingConstruction.buildingInteractions[currentPathElevatorBuilding.elevatorWaitingPassengers.Count].waypoints[0].transform.position);
                    }
                    else
                    {
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
                        navMeshAgent.SetDestination(nextElevatorBuilding.GetInteractionPointPosition());
                    }
                    else if (nextBuilding)
                    {
                        navMeshAgent.SetDestination(nextBuilding.transform.position);
                    }
                }
            }
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
    protected void OnBuildingStartConstructing(Building building)
    {
        StartCoroutine(OnBuildingStartConstructingCoroutine(building));
    }

    protected virtual IEnumerator OnBuildingStartConstructingCoroutine(Building building)
    {
        yield return new WaitForEndOfFrame();

        if (targetBuilding)
        {
            SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == targetBuilding.GetFloorIndex() && b.GetPlaceIndex() == targetBuilding.GetPlaceIndex());
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
