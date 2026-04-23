using System;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class BuildingModule : MonoBehaviour, IOwnedBuildingListener
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
                Debug.LogError(ownedBuilding.BuildingData.BuildingName + $" has no level data by index {level}");
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

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    public void OnOwnedBuildingInited()
    {
        OnInit();
        onBuildingModuleInited?.Invoke(this);
    }

    public void OnOwnedBuildingDemolished()
    {
        OnDemolish();
        onBuildingModuleDemolished?.Invoke(this);
    }

    protected abstract void OnInit();

    protected abstract void OnDemolish();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

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
        if (!ownedBuilding) return;

        Subscribe();

        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed) return;
        if (!ownedBuilding) return;

        Unsubscribe();

        isSubscribed = false;
    }
}