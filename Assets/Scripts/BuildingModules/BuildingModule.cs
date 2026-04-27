using System;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding => ownedBuilding ? ownedBuilding : GetComponent<Building>();

    protected bool isWorking { get; private set; } = false;

    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;
    public BuildingModuleLevelData LevelData
    {
        get
        {
            int level = ownedBuilding.LevelComponent.level - 1;

            if (level < LevelsData.Length) {
                return LevelsData[level];
            }
            else {
                Debug.LogError(ownedBuilding.BuildingData.BuildingId.ToString() + $" has no level data by index {level}");
                return null;
            }
        }
    }
    protected BuildingConstruction BuildingConstruction => ownedBuilding.spawnedConstruction;

    private bool isSubscribed = false;

    public static event Action<BuildingModule> onBuildingModuleInited;
    public static event Action<BuildingModule> onBuildingModuleUpgraded;
    public static event Action<BuildingModule> onBuildingModuleDemolished;
    public static event Action<BuildingModule> onBuildingModuleStartedWorking;
    public static event Action<BuildingModule> onBuildingModuleStoppedWorking;

    protected void Awake()
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
        ownedBuilding.onInited += OnBuildingInited;
    }

    protected virtual void Unsubscribe()
    {
        ownedBuilding.onInited -= OnBuildingInited;
    }

    protected abstract void OnInit();

    protected abstract void OnDemolish();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected void StartWorking()
    {
        if (isWorking) return;

        OnBuildingStartWorking();
        isWorking = true;

        onBuildingModuleStartedWorking?.Invoke(this);
    }

    protected void StopWorking()
    {
        if (!isWorking) return;

        OnBuildingStopWorking();

        isWorking = false;
        onBuildingModuleStoppedWorking?.Invoke(this);
    }

    protected void SetFlickingPower(float multiplier)
    {
        BuildingConstruction.SetFlickingPower(multiplier);
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        Subscribe();

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;

        Unsubscribe();

        isSubscribed = false;
    }

    private void OnBuildingInited()
    {
        OnInit();
        onBuildingModuleInited?.Invoke(this);
    }

    private void OnBuildingDemolished()
    {
        OnDemolish();
        onBuildingModuleDemolished?.Invoke(this);
    }
}