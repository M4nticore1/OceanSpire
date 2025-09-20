using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Resident : Entity
{
    [HideInInspector] public bool isWorker { get; private set; } = false;
    [HideInInspector] public bool isWorking { get; private set; } = false;
    [HideInInspector] public int workerIndex { get; private set; } = 0;
    [HideInInspector] public Building workBuilding { get; private set; } = null;

    private float actionTime = 0.0f;
    private int actionIndex = 0;

    public static event System.Action OnWorkerAdd;
    public static event System.Action OnWorkerRemove;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isWorker)
        {
            if (isWorking)
            {
                Work();
            }
            else
            {

            }
        }
    }

    public override void EnterBuilding(Building buildingPlace)
    {
        base.EnterBuilding(buildingPlace);

        if (currentBuilding == workBuilding)
        {
            StartWorking();
        }
    }

    public override void ExitBuilding()
    {
        base.ExitBuilding();
    }

    private void Work()
    {
        if (workBuilding.spawnedBuildingConstruction.buildingInteractions.Count > workerIndex)
        {
            BuildingAction buildingAction = workBuilding.spawnedBuildingConstruction.buildingInteractions[workerIndex];

            if (buildingAction.actionTimes[actionIndex] > 0)
            {
                actionTime += Time.deltaTime;

                if (actionTime >= buildingAction.actionTimes[actionIndex])
                {
                    if (actionIndex < buildingAction.actionTimes.Count - 1)
                        actionIndex++;
                    else
                        actionIndex = 0;

                    actionTime = 0;

                    navMeshAgent.SetDestination(buildingAction.waypoints[actionIndex].position);
                }
            }
        }
    }

    public override void SetTargetBuilding(Building targetBuilding)
    {
        base.SetTargetBuilding(targetBuilding);


    }

    public void SetWorkBuilding(Building building)
    {
        isWorker = true;
        workBuilding = building;
        workerIndex = building.workersCount;

        SetTargetBuilding(building);

        if (currentFloorIndex <= 1 && workBuilding.GetFloorIndex() <= 1)
        {
            if (workBuilding.spawnedBuildingConstruction.buildingInteractions.Count > workerIndex)
            {
                BuildingAction buildingAction = workBuilding.spawnedBuildingConstruction.buildingInteractions[workerIndex];

                if (buildingAction.waypoints.Count > 0)
                {
                    navMeshAgent.SetDestination(buildingAction.waypoints[0].position);
                }
                else
                {
                    Debug.Log("buildingActions.waypoints.Count == 0");
                }
            }
            else
            {
                navMeshAgent.SetDestination(workBuilding.transform.position);

                Debug.Log("buildingActions.Count <= workerIndex");
            }
        }

        building.AddWorker(this);

        OnWorkerAdd?.Invoke();
    }

    public void RemoveWorkBuilding()
    {
        isWorker = false;
        workBuilding.RemoveWorker(this);
        workBuilding = null;

        StopWorking();

        OnWorkerRemove?.Invoke();
    }

    public void SetWorkerIndex(int index)
    {
        workerIndex = index;
    }

    private void StartWorking()
    {
        isWorking = true;
    }

    private void StopWorking()
    {
        isWorking = false;
        navMeshAgent.ResetPath();
    }
}
