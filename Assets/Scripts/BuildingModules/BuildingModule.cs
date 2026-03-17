using UnityEngine;

public abstract class BuildingModule : MonoBehaviour, IOwnedBuildingListener
{
    protected BuildingsManager buildingsManager;

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

    protected void Awake()
    {
        ownedBuilding = GetComponent<Building>();
    }

    public void HandleOwnedBuildingInited()
    {
        buildingsManager = FindAnyObjectByType<BuildingsManager>();

        OnInit();
        EventBus.InvokeBuildingModuleInited(this);
    }

    public void HandleOwnedBuildingDemolished()
    {
        OnDemolish();
        EventBus.InvokeBuildingModuleDemolished(this);
    }

    protected abstract void OnInit();

    protected abstract void OnDemolish();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected void SetWorking(bool value)
    {
        if (value == isWorking) return;

        isWorking = value;
        if (isWorking) {
            OnBuildingStartWorking();
        }
        else {
            OnBuildingStopWorking();
        }
    }

    protected void SetFlickingPower(float multiplier)
    {
        BuildingConstruction.SetFlickingPower(multiplier);
    }
}
