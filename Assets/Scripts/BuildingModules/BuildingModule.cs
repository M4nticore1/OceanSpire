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

    protected virtual void Start()
    {

    }

    protected virtual void Subscribe()
    {
        ownedBuilding.OnInited += Init;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished += HandleUpgradeFinished;

        ownedBuilding.CitizensHandler.OnInteractorAdded += HandleWorkerAdded;
        ownedBuilding.CitizensHandler.OnInteractorRemoved += HandleWorkerRemoved;
        ownedBuilding.CitizensHandler.OnCurrentInteractorAdded += HandleCurrentWorkerAdded;
        ownedBuilding.CitizensHandler.OnCurrentInteractorRemoved += HandleCurrentWorkerRemoved;
    }

    protected virtual void Unsubscribe()
    {
        ownedBuilding.OnInited -= OnInit;
        OwnedBuilding.UpgradeComponent.OnUpgradeFinished -= HandleUpgradeFinished;

        ownedBuilding.CitizensHandler.OnInteractorAdded -= HandleWorkerAdded;
        ownedBuilding.CitizensHandler.OnInteractorRemoved -= HandleWorkerRemoved;
        ownedBuilding.CitizensHandler.OnCurrentInteractorAdded -= HandleCurrentWorkerAdded;
        ownedBuilding.CitizensHandler.OnCurrentInteractorRemoved -= HandleCurrentWorkerRemoved;
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

    protected virtual void HandleWorkingStart()
    {

    }

    protected virtual void HandleWorkingStop()
    {

    }

    protected virtual bool ShouldStartWorking()
    {
        if (IsWorking) return false;
        if (ownedBuilding.CitizensHandler.CurrentInteractors.Count <= 0) return false;

        return true;
    }

    protected virtual bool ShouldStopWorking()
    {
        if (!IsWorking) return false;
        if (OwnedBuilding.CitizensHandler.CurrentInteractors.Count <= 0) return true;

        return false;
    }

    public bool TryStartWorking()
    {
        if (!ShouldStartWorking()) return false;

        StartWorking();
        return true;
    }

    public bool TryStopWorking()
    {
        if (!ShouldStopWorking()) return false;

        StopWorking();
        return true;
    }

    private void HandleWorkerAdded(Human human)
    {
        TryStartWorking();
    }

    private void HandleWorkerRemoved(Human human)
    {
        TryStopWorking();
    }

    private void HandleCurrentWorkerAdded(Human human)
    {
        TryStartWorking();
    }

    private void HandleCurrentWorkerRemoved(Human human)
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
        IsWorking = true;
        HandleWorkingStart();

        OnWorkingStarted?.Invoke();
    }

    private void StopWorking()
    {
        IsWorking = false;
        HandleWorkingStop();

        OnWorkingStopped?.Invoke();
    }
}