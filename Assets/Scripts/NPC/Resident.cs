using System.Linq;
using System.Collections.Generic;
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

    // Movement
    public override void EnterBuilding(Building buildingPlace)
    {
        base.EnterBuilding(buildingPlace);

        if (currentBuilding == targetBuilding)
        {
            targetBuilding = null;

            if (isWorker)
            {
                StartWorking();
            }
        }
    }

    public override void ExitBuilding()
    {
        base.ExitBuilding();
    }

    // Work
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

    public void SetWorkBuilding(Building building)
    {
        isWorker = true;
        workBuilding = building;
        workerIndex = building.workers.Count;

        SetTargetBuilding(b => b.GetFloorIndex() == building.GetFloorIndex() && b.GetPlaceIndex() == building.GetPlaceIndex());

        building.AddWorker(this);

        OnWorkerAdd?.Invoke();
    }

    public void RemoveWorkBuilding()
    {
        isWorker = false;
        workBuilding.RemoveWorker(this);
        workBuilding = null;

        StopWorking();

        if(targetBuilding)
            targetBuilding = null;

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

    private void StartConstructingBuilding()
    {

    }

    // Buildings
    protected override void OnBuildingStartConstructing(Building building)
    {
        base.OnBuildingStartConstructing(building);

        if (!targetBuilding)
        {
            if (!workBuilding)
            {
                SetTargetBuilding(b =>
                {
                    StorageBuildingComponent storage = b.GetComponent<StorageBuildingComponent>();
                    if (!storage) return false;

                    int itemIndex = GameManager.GetItemIndexById(storage.storedItems.Select(x => x.itemData).ToList(), (int)building.buildingLevelsData[building.levelIndex].resourcesToBuild[0].itemData.itemId);

                    return itemIndex >= 0 && storage.storedItems[itemIndex].amount > 0;
                });
            }
        }
    }
}
