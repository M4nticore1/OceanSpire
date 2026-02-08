using UnityEngine;

public abstract class BuildingModule : MonoBehaviour
{
    private Building ownedBuilding = null;
    public Building OwnedBuilding { get { return ownedBuilding != null ? ownedBuilding : GetComponent<Building>(); } }

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
        ownedBuilding.onBuildingInited += OnBuildingInited;
        ownedBuilding.onBuildingStartWorking += OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking += OnBuildingStopWorking;
        ownedBuilding.onEntityEnterBuilding += OnEnterBuilding;
        ownedBuilding.onEntityExitBuilding += OnExitBuilding;
    }

    protected virtual void OnDisable()
    {
        ownedBuilding.onBuildingInited -= OnBuildingInited;
        ownedBuilding.onBuildingStartWorking -= OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking -= OnBuildingStopWorking;
        ownedBuilding.onEntityEnterBuilding -= OnEnterBuilding;
        ownedBuilding.onEntityExitBuilding -= OnExitBuilding;
    }

    private void OnBuildingInited()
    {
        OnInit();
        EventBus.InvokeBuildingModuleInited(this);
    }

    protected abstract void OnInit();

    protected abstract void OnBuildingStartWorking();

    protected abstract void OnBuildingStopWorking();

    protected abstract void OnEnterBuilding();

    protected abstract void OnExitBuilding();

    protected virtual void SetFlickingMultiplier(float multiplier)
    {
        BuildingConstruction.SetFlickingMultiplier(multiplier);
    }
}
