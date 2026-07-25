using System;
using UnityEngine;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding;
    public Building OwnedBuilding => ownedBuilding ? ownedBuilding : GetComponent<Building>();
    public TowerBuilding OwnedTowerBuilding => OwnedBuilding as TowerBuilding;

    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;

    public BuildingModuleLevelData LastLevelData
    {
        get {
            if (!ownedBuilding) {
                Debug.LogError($"[{nameof(BuildingModule)}] Owned Building is not valid!");
                return null;
            }

            int index = ownedBuilding.LevelComponent.Level - 2;
            if (index >= 0 && index < LevelsData.Length) {
                return LevelsData[index];
            }

            return null;
        }
    }

    public BuildingModuleLevelData LevelData
    {
        get {
            if (!ownedBuilding) {
                Debug.LogError($"[{nameof(BuildingModule)}] Owned Building is not valid on {name}!");
                return null;
            }

            int index = ownedBuilding.LevelComponent.Level - 1;
            if (index >= 0 && index < LevelsData.Length) {
                return LevelsData[index];
            }

            Debug.LogError($"[{nameof(BuildingModule)}] Has no level data at index {index}!");
            return null;
        }
    }

    protected BuildingConstruction BuildingConstruction => ownedBuilding.SpawnedConstruction;

    protected bool IsInited { get; private set; } = false;
    private bool isSubscribed = false;
    [field: SerializeField] public bool IsWorking { get; private set; } = false;

    public event Action OnInited;

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
        ownedBuilding.OnInited += Init;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished += HandleUpgradeFinished;

        ownedBuilding.WorkComponent.OnWorkerAdded += HandleWorkerAdded;
        ownedBuilding.WorkComponent.OnWorkerRemoved += HandleWorkerRemoved;
        ownedBuilding.WorkComponent.OnCurrentWorkerAdded += HandleCurrentWorkerAdded;
        ownedBuilding.WorkComponent.OnCurrentWorkerRemoved += HandleCurrentWorkerRemoved;
    }

    protected virtual void Unsubscribe()
    {
        ownedBuilding.OnInited -= OnInit;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished -= HandleUpgradeFinished;

        ownedBuilding.WorkComponent.OnWorkerAdded -= HandleWorkerAdded;
        ownedBuilding.WorkComponent.OnWorkerRemoved -= HandleWorkerRemoved;
        ownedBuilding.WorkComponent.OnCurrentWorkerAdded -= HandleCurrentWorkerAdded;
        ownedBuilding.WorkComponent.OnCurrentWorkerRemoved -= HandleCurrentWorkerRemoved;
    }

    protected virtual bool ShouldSubscribe()
    {
        if (isSubscribed) return false;

        return true;
    }

    protected virtual bool ShouldUnsubscribe()
    {
        if (!isSubscribed) return false;

        return true;
    }

    private void Init()
    {
        OnInit();
        IsInited = true;
        OnInited?.Invoke();
    }

    protected virtual void OnInit()
    {

    }

    protected virtual void HandleUpgradeFinished()
    {
        TryStartWorking();
    }

    protected virtual void OnWorkingStart()
    {

    }

    protected virtual void OnWorkingStop()
    {

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

    private void HandleWorkerAdded(Citizen citizen)
    {
        TryStartWorking();
    }

    private void HandleWorkerRemoved(Citizen citizen)
    {
        TryStopWorking();
    }

    private void HandleCurrentWorkerAdded(Citizen citizen)
    {
        TryStartWorking();
    }

    private void HandleCurrentWorkerRemoved(Citizen citizen)
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