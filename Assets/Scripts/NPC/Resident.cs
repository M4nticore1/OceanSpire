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

        if (isWorking)
        {
            Work();
        }
    }

    // Movement
    public override void EnterBuilding(Building buildingPlace)
    {
        base.EnterBuilding(buildingPlace);

        if (currentBuilding == targetBuilding)
        {
            targetBuilding = null;

            if (currentWork != ResidentWork.None)
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
        if (currentWork == ResidentWork.BuildingWork)
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
        else if (currentWork == ResidentWork.ConstructingBuilding)
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

    //public void SetWorkBuilding(Building building)
    //{
    //    if (building)
    //    {
    //        workBuilding = building;
    //        workerIndex = building.workers.Count;

    //        SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == building.GetFloorIndex() && b.GetPlaceIndex() == building.GetPlaceIndex(), ResidentWork.BuildingWork);

    //        building.AddWorker(this);

    //        OnWorkerAdd?.Invoke();
    //    }
    //}

    public void RemoveWorkBuilding()
    {
        currentWork = ResidentWork.None;
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

    public void SetWork(ResidentWork newWork, Building newWorkBuilding)
    {
        currentWork = newWork;

        if (newWorkBuilding)
        {
            workBuilding = newWorkBuilding;

            if (newWork == ResidentWork.BuildingWork)
            {
                workBuilding = newWorkBuilding;
                newWorkBuilding.AddWorker(this);

                SetTargetBuilding(currentBuilding ? currentBuilding.buildingPlace : null, b => b.GetFloorIndex() == newWorkBuilding.GetFloorIndex() && b.GetPlaceIndex() == newWorkBuilding.GetPlaceIndex());

                OnWorkerAdd?.Invoke();
            }
            else if (newWork == ResidentWork.ConstructingBuilding)
            {
                if (SetTargetBuilding(newWorkBuilding.buildingPlace, b =>
                {
                    if (!b.storageComponent || (b.GetFloorIndex() == newWorkBuilding.GetFloorIndex() && b.GetPlaceIndex() == newWorkBuilding.GetPlaceIndex())) return false;

                    int itemIndex = (int)newWorkBuilding.buildingLevelsData[newWorkBuilding.levelIndex].resourcesToBuild[0].itemData.itemId;

                    return b.storageComponent.storedItems.ContainsKey(itemIndex) && b.storageComponent.storedItems[itemIndex] >= 0;
                }))
                {
                    StartWorking();
                }
            }
        }
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
        base.OnBuildingStartConstructingCoroutine(building);

        yield return new WaitForEndOfFrame();

        if (currentWork == ResidentWork.None)
        {
            SetWork(ResidentWork.ConstructingBuilding, building);
        }
    }
}
