using UnityEngine;

public abstract class BuildingComponent : MonoBehaviour
{
    public Building ownedBuilding { get; protected set; } = null;

    protected int levelIndex => ownedBuilding.LevelIndex;
    [SerializeField] protected BuildingComponentLevelData[] levelsData = { };
    public BuildingComponentLevelData[] LevelsData => levelsData;
    public BuildingComponentLevelData LevelData => levelsData[ownedBuilding.LevelIndex];
    protected BuildingConstruction BuildingConstruction => ownedBuilding.constructionComponent.SpawnedConstruction;

    protected void Awake()
    {
        ownedBuilding = GetComponent<Building>();
    }

    protected virtual void OnEnable()
    {
        ownedBuilding.onBuildingFinishConstructing += BuildComponent;
        ownedBuilding.onBuildingStartWorking += OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking += OnBuildingStopWorking;
        ownedBuilding.onEnterBuilding += OnEnterBuilding;
        ownedBuilding.onExitBuilding += OnExitBuilding;
        ownedBuilding.onResidentStartWorking += OnResidentStartWorking;
        ownedBuilding.onResidentStopWorking += OnResidentStopWorking;
    }

    protected virtual void OnDisable()
    {
        ownedBuilding.onBuildingStartWorking -= OnBuildingStartWorking;
        ownedBuilding.onBuildingStopWorking -= OnBuildingStopWorking;
        ownedBuilding.onEnterBuilding -= OnEnterBuilding;
        ownedBuilding.onExitBuilding -= OnExitBuilding;
        ownedBuilding.onResidentStartWorking -= OnResidentStartWorking;
        ownedBuilding.onResidentStopWorking -= OnResidentStopWorking;
    }

    public void Initialize()
    {

    }

    protected virtual void BuildComponent()
    {

    }

    protected virtual void OnBuildingStartWorking()
    {

    }

    protected virtual void OnBuildingStopWorking()
    {

    }

    protected virtual void OnEnterBuilding()
    {

    }

    protected virtual void OnExitBuilding()
    {

    }

    protected virtual void OnResidentStartWorking()
    {

    }

    protected virtual void OnResidentStopWorking()
    {

    }

    protected void SetFlickingMultiplier(float multiplier)
    {
        BuildingConstruction.SetFlickingMultiplier(multiplier);
    }

    //public virtual void UpdateLevel(int newLevel)
    //{

    //}
}
