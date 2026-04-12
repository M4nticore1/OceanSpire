using System;
using UnityEngine;

public abstract class BuildingModule : MonoBehaviour, IOwnedBuildingListener
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding => ownedBuilding != null ? ownedBuilding : GetComponent<Building>();

    protected bool isWorking { get; private set; } = false;

    protected int LevelIndex => OwnedBuilding.LevelIndex;
    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;
    public BuildingModuleLevelData LevelData
    {
        get
        {
            if (LevelIndex < LevelsData.Length)
                return LevelsData[LevelIndex];
            else {
                Debug.LogError(ownedBuilding.BuildingData.BuildingName + $" has no level data by index {LevelIndex}");
                return null;
            }
        }
    }
    protected BuildingConstruction BuildingConstruction => ownedBuilding.spawnedConstruction;

    public static event Action<BuildingModule> onBuildingModuleInited;
    public static event Action<BuildingModule> onBuildingModuleUpgraded;
    public static event Action<BuildingModule> onBuildingModuleDemolished;
    public static event Action<BuildingModule> onBuildingModuleStartedWorking;
    public static event Action<BuildingModule> onBuildingModuleStoppedWorking;

    protected void Awake()
    {
        ownedBuilding = GetComponent<Building>();
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
}
