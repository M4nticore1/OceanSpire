using UnityEngine;

public abstract class BuildingModule : MonoBehaviour, IOwnedBuildingListener
{
    protected BuildingsManager buildingsManager;

    private Building ownedBuilding = null;
    public Building OwnedBuilding => ownedBuilding != null ? ownedBuilding : GetComponent<Building>();

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

    protected virtual void OnEnable()
    {
        ownedBuilding.onBuildingStartWorking += OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking += OnBuildingStopWorking;
        ownedBuilding.onEntityEnterBuilding += OnEnterBuilding;
        ownedBuilding.onEntityExitBuilding += OnExitBuilding;
    }

    protected virtual void OnDisable()
    {
        ownedBuilding.onBuildingStartWorking -= OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking -= OnBuildingStopWorking;
        ownedBuilding.onEntityEnterBuilding -= OnEnterBuilding;
        ownedBuilding.onEntityExitBuilding -= OnExitBuilding;
    }

    protected abstract void OnInit();

    protected abstract void OnDemolish();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected abstract void OnEnterBuilding(EntityCityNavigator navigator);

    protected abstract void OnExitBuilding(EntityCityNavigator navigator);

    protected virtual void SetFlickingMultiplier(float multiplier)
    {
        BuildingConstruction.SetFlickingMultiplier(multiplier);
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
}
