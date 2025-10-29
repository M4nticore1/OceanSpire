using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Resident : Entity
{
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
                if (currentBuilding == targetBuilding)
                {
                    float distance = Vector3.Distance(transform.position, targetPosition);
                    if (distance < applyTargetPosition && navMeshAgent.velocity == Vector3.zero)
                    {

                    }
                }
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
        if (building)
        {
            isWorker = true;
            workBuilding = building;
            workerIndex = building.workers.Count;

            SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == building.GetFloorIndex() && b.GetPlaceIndex() == building.GetPlaceIndex());

            building.AddWorker(this);

            OnWorkerAdd?.Invoke();
        }
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
    protected override IEnumerator OnBuildingStartConstructingCoroutine(Building building)
    {
        base.EnterBuilding(building);

        yield return new WaitForEndOfFrame();

        if (!targetBuilding)
        {
            if (!workBuilding)
            {
                if (SetTargetBuilding(building.buildingPlace, b =>
                {
                    if (!b.storageComponent || (b.GetFloorIndex() == building.GetFloorIndex() && b.GetPlaceIndex() == building.GetPlaceIndex())) return false;

                    int itemIndex = (int)building.buildingLevelsData[building.levelIndex].resourcesToBuild[0].itemData.itemId;

                    return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex] >= 0;
                }))
                {
                    StartWorking();
                }
            }
        }
    }
}
