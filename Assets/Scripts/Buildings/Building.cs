using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("Buildings/Building")]
public class Building : MonoBehaviour
{
    protected GameManager gameManager { get; private set; } = null;
    public CityManager cityManager { get; private set; } = null;
    public LevelComponent levelComponent { get; private set; } = null;
    public ConstructionComponent constructionComponent { get; private set; } = null;
    public SelectComponent selectComponent { get; private set; } = null;
    public StorageBuildingComponent storageComponent { get; private set; } = null;
    public ProductionBuildingComponent productionComponent { get; private set; } = null;
    public bool isInitialized { get; private set; } = false;
    protected bool isSelected { get; private set; } = false;

    //[HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Entity> workers { get; private set; } = new List<Entity>();
    public List<Entity> currentWorkers { get; private set; } = new List<Entity>();

    [Header("Data")]
    [SerializeField] protected BuildingData buildingData = null;
    public BuildingData BuildingData => buildingData;
    [SerializeField] protected ConstructionLevelData[] buildingLevelsData = { };
    public ConstructionLevelData[] ConstructionLevelsData => buildingLevelsData;

    [HideInInspector] public BuildingPlace buildingPlace = null;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        levelComponent = GetComponent<LevelComponent>();
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuildingComponent>();
    }

    protected void OnEnable()
    {
        constructionComponent.onBuildingStartConstructing += OnBuildingStartConstructing;
        constructionComponent.onBuildingFinishConstructing += OnBuildingFinishConstructing;
        constructionComponent.onConstructionDemolished += OnConstructionDemolised;
    }

    protected void OnDisable()
    {
        constructionComponent.onBuildingStartConstructing -= OnBuildingStartConstructing;
        constructionComponent.onBuildingFinishConstructing -= OnBuildingFinishConstructing;
        constructionComponent.onConstructionDemolished -= OnConstructionDemolised;
    }

    protected void Update()
    {

    }

    // Constructing
    public virtual void InitializeBuilding(BuildingPlace buildingPlace)
    {
        //gameManager = FindAnyObjectByType<GameManager>();
        //cityManager = FindAnyObjectByType<CityManager>();
        //levelComponent = GetComponent<LevelComponent>();
        //constructionComponent = GetComponent<ConstructionComponent>();
        //storageComponent = GetComponent<StorageBuildingComponent>();
        //productionComponent = GetComponent<ProductionBuildingComponent>();

        this.buildingPlace = buildingPlace;
        isInitialized = true;
    }

    protected virtual void Place(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    {
        InitializeBuilding(buildingPlace);

        //if (levelComponent)
        //{
        //    levelComponent.levelIndex = levelIndex;
        //    if (constructionComponent)
        //        constructionComponent.Place(buildingPlace, levelIndex, requiresConstruction, interiorIndex);
        //}
    }

    protected void OnBuildingStartConstructing()
    {
        UpdateBuildingConstruction(levelComponent.levelIndex);
    }

    protected virtual void OnBuildingFinishConstructing()
    {
        if (storageComponent)
            storageComponent.Build(levelComponent.levelIndex);
        if (productionComponent)
            productionComponent.Build(levelComponent.levelIndex);

        UpdateBuildingConstruction(levelComponent.levelIndex);
    }

    protected void OnConstructionDemolised()
    {
        buildingPlace.DestroyBuilding();
    }

    // Residents Management
    public virtual void EnterBuilding(Entity entity)
    {
        entities.Add(entity);

        Resident resident = entity as Resident;

        if (resident)
        {
            if (resident.isWorking) // If constructing building
            {
                if (resident.pathBuildings[resident.pathBuildings.Count - 1])
                {

                }
            }
            else if (resident.currentWork != ResidentWork.None) // If worker of some building
            {
                if (resident.workBuilding == this)
                    AddCurrentWorker(resident);
            }
        }
    }

    public virtual void ExitBuilding(Entity entity)
    {
        entities.Remove(entity);
    }

    public void AddWorker(Entity worker)
    {
        workers.Add(worker);
        worker.SetWorkerIndex(workers.Count - 1);
    }

    public void RemoveWorker(Entity worker)
    {
        workers.RemoveAt(worker.workerIndex);

        for (int i = 0; i < workers.Count; i++)
        {
            workers[i].SetWorkerIndex(i);
        }
    }

    public void AddCurrentWorker(Entity worker)
    {
        currentWorkers.Add(worker);
    }

    public void RemoveCurrentWorker(Resident worker)
    {
        currentWorkers.RemoveAt(worker.workerIndex);

        for (int i = 0; i < currentWorkers.Count; i++)
        {
            currentWorkers[i].SetWorkerIndex(i);
        }
    }

    protected virtual void UpdateBuildingConstruction(int levelIndex)
    {
        BuildConstruction(levelIndex);
    }

    protected virtual void BuildConstruction(int levelIndex)
    {

    }

    public int GetFloorIndex()
    {
        if (buildingPlace)
            return buildingPlace.floorIndex;
        else
            return 0;
    }

    public int GetPlaceIndex()
    {
        if (buildingPlace)
            return buildingPlace.buildingPlaceIndex;
        else
            return 0;
    }

    public int GetTotalIndex()
    {
        if (buildingPlace)
            return GetFloorIndex() * GetPlaceIndex() + GetPlaceIndex();
        else
            return 0;
    }

    public Vector3 GetInteractionPosition()
    {
        List<BuildingAction> buildingInteraction = constructionComponent.spawnedConstruction.buildingInteractions;
        if (buildingInteraction.Count > 0 && buildingInteraction[0].waypoints.Count > 0)
            return buildingInteraction[0].waypoints[0].position;
        else
            return transform.position;
    }

    public Vector3 GetPickupItemPointPosition()
    {
        if (constructionComponent.spawnedConstruction.collectItemPoints.Count > 0)
            return constructionComponent.spawnedConstruction.collectItemPoints[0].position;
        else
            return transform.position;
    }
}
