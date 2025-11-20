using System.Collections.Generic;
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
    [SerializeField] protected List<ConstructionLevelData> buildingLevelsData = new List<ConstructionLevelData>();
    public List<ConstructionLevelData> ConstructionLevelsData => buildingLevelsData;

    [HideInInspector] public BuildingPlace buildingPlace = null;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        levelComponent = GetComponent<LevelComponent>();
        selectComponent = GetComponent<SelectComponent>();
        constructionComponent = GetComponent<ConstructionComponent>();
        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuildingComponent>();
    }

    protected virtual void OnEnable()
    {
        constructionComponent.onBuildingStartConstructing += StartConstructing;
        constructionComponent.onBuildingFinishConstructing += FinishConstructing;
        constructionComponent.onConstructionDemolished += OnConstructionDemolised;
    }

    protected virtual void OnDisable()
    {
        constructionComponent.onBuildingStartConstructing -= StartConstructing;
        constructionComponent.onBuildingFinishConstructing -= FinishConstructing;
        constructionComponent.onConstructionDemolished -= OnConstructionDemolised;
    }

    protected virtual void Start()
    {
        Place();
    }

    protected void Update()
    {

    }

    // Constructing
    public virtual void InitializeBuilding(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    {
        if (isInitialized) return;

        if (BuildingData.BuildingIdName == "tower_gate")
            Debug.Log("initialize");

        constructionComponent.InitializeConstruction(levelIndex, requiresConstruction);
        this.buildingPlace = buildingPlace;
        isInitialized = true;
    }

    protected virtual void Place(/*BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex*/)
    {
        if (constructionComponent)
        {
            if (constructionComponent.isUnderConstruction)
                StartConstructing();
            else
                FinishConstructing();
        }
        else
            Debug.LogError(BuildingData.BuildingName + " has no constructionComponent");
    }

    protected void StartConstructing()
    {
        UpdateBuildingConstruction(levelComponent.LevelIndex);
    }

    protected virtual void FinishConstructing()
    {
        if (levelComponent)
        {
            if (storageComponent)
                storageComponent.Build(levelComponent.LevelIndex);
            if (productionComponent)
                productionComponent.Build(levelComponent.LevelIndex);

            UpdateBuildingConstruction(levelComponent.LevelIndex);
        }
        else
        {
            Debug.LogError(buildingData.BuildingName + " has no level component");
        }
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
        Debug.Log("UpdateBuildingConstruction");

        BuildConstruction(levelIndex);
    }

    public virtual void BuildConstruction(int levelIndex)
    {
        constructionComponent.BuildConstruction(buildingLevelsData[levelIndex].ConstructionStraight);
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
        if (!constructionComponent.spawnedConstruction)
            Debug.Log("alaalaala");

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
