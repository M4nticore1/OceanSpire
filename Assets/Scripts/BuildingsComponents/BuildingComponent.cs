using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class BuildingComponent : MonoBehaviour
{
    protected GameManager gameManager { get; private set; } = null;
    protected CityManager cityManager { get; private set; } = null;
    [SerializeField, HideInInspector] public Building ownedBuilding = null;

    protected int levelIndex => ownedBuilding.levelIndex;
    [SerializeField] protected BuildingComponentLevelData[] levelsData = { };
    public BuildingComponentLevelData[] LevelsData => levelsData;
    public BuildingComponentLevelData LevelData => levelsData[ownedBuilding.levelIndex];
    protected BuildingConstruction BuildingConstruction => ownedBuilding.constructionComponent.SpawnedConstruction;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
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
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        ownedBuilding = GetComponent<Building>();
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
