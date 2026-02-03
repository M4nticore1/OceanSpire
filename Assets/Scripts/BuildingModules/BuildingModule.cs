using UnityEngine;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding { get { return ownedBuilding != null ? ownedBuilding : GetComponent<Building>(); } }

    protected int LevelIndex => OwnedBuilding.LevelIndex;
    [SerializeField] protected BuildingModuleLevelData[] levelsData = { };
    public BuildingModuleLevelData[] LevelsData => levelsData;
    public BuildingModuleLevelData LevelData => levelsData[ownedBuilding.LevelIndex];
    protected BuildingConstruction BuildingConstruction => ownedBuilding.spawnedConstruction;

    protected void Awake()
    {
        ownedBuilding = GetComponent<Building>();
    }

    protected virtual void OnEnable()
    {
        ownedBuilding.onBuildingInited += OnBuildingInited;
        ownedBuilding.onBuildingStartWorking += OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking += OnBuildingStopWorking;
        ownedBuilding.onEnterBuilding += OnEnterBuilding;
        ownedBuilding.onExitBuilding += OnExitBuilding;
        ownedBuilding.onResidentStartWorking += OnResidentStartWorking;
        ownedBuilding.onResidentStopWorking += OnResidentStopWorking;
    }

    protected virtual void OnDisable()
    {
        ownedBuilding.onBuildingInited -= OnBuildingInited;
        ownedBuilding.onBuildingStartWorking -= OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking -= OnBuildingStopWorking;
        ownedBuilding.onEnterBuilding -= OnEnterBuilding;
        ownedBuilding.onExitBuilding -= OnExitBuilding;
        ownedBuilding.onResidentStartWorking -= OnResidentStartWorking;
        ownedBuilding.onResidentStopWorking -= OnResidentStopWorking;
    }

    protected abstract void OnBuildingInited();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected abstract void OnEnterBuilding();

    protected abstract void OnExitBuilding();

    protected abstract void OnResidentStartWorking();

    protected abstract void OnResidentStopWorking();

    protected virtual void SetFlickingMultiplier(float multiplier)
    {
        BuildingConstruction.SetFlickingMultiplier(multiplier);
    }
}
