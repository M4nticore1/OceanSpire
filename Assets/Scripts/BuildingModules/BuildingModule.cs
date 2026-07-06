using System;
using UnityEngine;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding => ownedBuilding ? ownedBuilding : GetComponent<Building>();
    public TowerBuilding OwnedTowerBuilding => OwnedBuilding as TowerBuilding;

    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;
    public BuildingModuleLevelData LevelData
    {
        get
        {
            if (!ownedBuilding) {
                Debug.LogError("OwnedBuilding is not valid ", this);
                return null;
            }

            int level = ownedBuilding.LevelComponent.Level - 1;

            if (level < LevelsData.Length) {
                return LevelsData[level];
            }
            else {
                Debug.LogError(ownedBuilding.BuildingData.BuildingId.ToString() + $" has no level data by index {level}");
                return null;
            }
        }
    }
    protected BuildingConstruction BuildingConstruction => ownedBuilding.SpawnedConstruction;

    protected bool IsInited { get; private set; } = false;
    private bool isSubscribed = false;
    public bool IsWorking { get; private set; } = false;

    public event Action OnWorkingStarted;
    public event Action OnWorkingStopped;

    protected virtual void Awake()
    {
        ownedBuilding = GetComponent<Building>();
    }

    protected virtual void OnEnable()
    {
        TrySubscribe();
    }

    protected virtual void OnDisable()
    {
        TryUnsubscribe();
    }

    protected virtual void Subscribe()
    {
        ownedBuilding.OnInited += OnInit;
        ownedBuilding.WorkComponent.OnWorkerAdded += OnWorkerAdded;
        ownedBuilding.WorkComponent.OnWorkerRemoved += OnWorkerRemoved;
        ownedBuilding.WorkComponent.OnCurrentWorkerAdded += OnCurrentWorkerAdded;
        ownedBuilding.WorkComponent.OnCurrentWorkerRemoved += OnCurrentWorkerRemoved;
    }

    protected virtual void Unsubscribe()
    {
        ownedBuilding.OnInited -= OnInit;
        ownedBuilding.WorkComponent.OnWorkerAdded -= OnWorkerAdded;
        ownedBuilding.WorkComponent.OnWorkerRemoved -= OnWorkerRemoved;
        ownedBuilding.WorkComponent.OnCurrentWorkerAdded -= OnCurrentWorkerAdded;
        ownedBuilding.WorkComponent.OnCurrentWorkerRemoved -= OnCurrentWorkerRemoved;
    }

    protected virtual void OnInit()
    {
        IsInited = true;
    }

    protected virtual void OnWorkingStart()
    {

    }

    protected virtual void OnWorkingStop()
    {

    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;
        if (!ownedBuilding) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;
        if (!ownedBuilding) return false;

        return true;
    }

    protected virtual bool ShouldStartWorking()
    {
        if (ownedBuilding.WorkComponent.CurrentWorkers.Count <= 0) return false;

        return true;
    }

    protected virtual bool ShouldStopWorking()
    {
        if (OwnedBuilding.WorkComponent.CurrentWorkers.Count <= 0) return true;

        return false;
    }

    protected bool TryStartWorking()
    {
        if (!ShouldStartWorking()) return false;

        StartWorking();
        return true;
    }

    protected bool TryStopWorking()
    {
        if (!ShouldStopWorking()) return false;

        StopWorking();
        return true;
    }

    private void OnWorkerAdded(Citizen citizen)
    {
        TryStartWorking();
    }

    private void OnWorkerRemoved(Citizen citizen)
    {
        TryStopWorking();
    }

    private void OnCurrentWorkerAdded(Citizen citizen)
    {
        TryStartWorking();
    }

    private void OnCurrentWorkerRemoved(Citizen citizen)
    {
        TryStopWorking();
    }

    private bool TrySubscribe()
    {
        if (!ShouldSubscribe()) return false;

        Subscribe();
        isSubscribed = true;

        return true;
    }

    private bool TryUnsubscribe()
    {
        if (!ShouldUnsubscribe()) return false;

        Unsubscribe();
        isSubscribed = false;

        return true;
    }

    private void StartWorking()
    {
        if (IsWorking) return;

        IsWorking = true;
        OnWorkingStart();
        OnWorkingStarted?.Invoke();
    }

    private void StopWorking()
    {
        if (!IsWorking) return;

        IsWorking = false;
        OnWorkingStop();
        OnWorkingStopped?.Invoke();
    }
}