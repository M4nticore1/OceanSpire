using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static BuildingData;

public enum BuildingPosition
{
    Straight,
    Corner
}

[AddComponentMenu("")]
public class Building : MonoBehaviour
{
    protected GameManager gameManager = null;
    public CityManager cityManager = null;
    public bool isInitialized = false;

    [HideInInspector] public int levelIndex { get; private set; } = 0;
    [HideInInspector] public bool isUnderConstruction { get; private set; } = false;
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Resident> workers { get; private set; } = new List<Resident>();
    public List<Resident> currentWorkers { get; private set; } = new List<Resident>();

    [Header("Data")]
    public BuildingData buildingData = null;

    public BuildingLevelData[] buildingLevelsData = { };

    public BuildingPlace buildingPlace = null;

    [Header("Construction")]
    public BuildingConstruction spawnedBuildingConstruction = null;
    public bool isRuined = false;
    private GameObject spawedBuildingInterior = null;
    [HideInInspector] public int interiorIndex = 0;

    public static event Action<Building> onAnyBuildingStartConstructing;
    public event Action onBuildingStartConstructing;
    public static event Action<Building> onAnyBuildingFinishConstructing;
    public event Action onBuildingFinishConstructing;

    protected virtual void Awake()
    {
        //gameManager = FindAnyObjectByType<GameManager>();
        //cityManager = FindAnyObjectByType<CityManager>();
    }

    protected virtual void Start()
    {
        //InitializeBuilding(buildingPlace);

        //if (isUnderConstruction)
        //    StartBuilding(levelIndex);
        //else
        //    Build(levelIndex, interiorIndex);
    }

    protected virtual void Update()
    {

    }

    public virtual void InitializeBuilding(BuildingPlace buildingPlace)
    {
        this.buildingPlace = buildingPlace;
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
        isInitialized = true;
    }

    public virtual void Place(BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction, int interiorIndex)
    {
        InitializeBuilding(buildingPlace);

        this.levelIndex = levelIndex;
        this.isUnderConstruction = isUnderConstruction;
        this.interiorIndex = interiorIndex;

        if (isUnderConstruction)
            StartBuilding(levelIndex);
        else
            Build(levelIndex, interiorIndex);
    }

    public virtual void StartBuilding(int nextLevel)
    {
        isUnderConstruction = true;

        UpdateBuildingConstruction(nextLevel);

        if (GetType() == typeof(Building))
            InvokeStartConstructing(this);
    }

    public void FinishBuilding()
    {
        isUnderConstruction = false;

        if (isRuined)
            isRuined = false;
        else
            levelIndex++;

        interiorIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingInteriors.Count);

        Build(levelIndex, interiorIndex);
    }

    public virtual void Build(int newLevelIndex, int interiorIndex)
    {
        StorageBuildingComponent storageBuilding = GetComponent<StorageBuildingComponent>();
        ProductionBuildingComponent productionBuildingComponent = GetComponent<ProductionBuildingComponent>();

        if (storageBuilding)
            storageBuilding.Build();
        if(productionBuildingComponent)
            productionBuildingComponent.Build();

        UpdateBuildingConstruction(levelIndex);

        if (spawnedBuildingConstruction && spawnedBuildingConstruction.buildingInteriors.Count > 0)
        {
            if (interiorIndex < 0)
                interiorIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingInteriors.Count);

            spawedBuildingInterior = Instantiate(spawnedBuildingConstruction.buildingInteriors[interiorIndex], transform);
        }

        //if (GetType() == typeof(Building))
        InvokeFinishConstructing(this);
    }

    public virtual void Demolish()
    {
        List<ResourceToBuild> resourceToBuilds = buildingLevelsData[levelIndex].ResourcesToBuild;

        for (int i = 0; i < resourceToBuilds.Count; i++)
        {
            int itemIndex = gameManager.GetItemIndexByIdName(resourceToBuilds[i].resourceData.itemIdName);
            int itemAmount = (int)math.ceil(resourceToBuilds[i].amount * GameManager.demolitionResourceRefundRate);

            cityManager.AddItemByIndex(itemIndex, itemAmount);
        }

        onAnyBuildingFinishConstructing?.Invoke(this);
        buildingPlace.DestroyBuilding();
        Destroy(gameObject);
    }

    protected virtual void UpdateBuildingConstruction(int levelIndex)
    {

    }

	protected virtual void BuildConstruction(int levelIndex)
	{
		if (spawnedBuildingConstruction)
        {
            Destroy(spawnedBuildingConstruction.gameObject);
        }
    }

    public virtual void EnterBuilding(Entity entity)
    {
        entities.Add(entity);

        Resident resident = entity as Resident;

        if (resident)
        {
            if (resident.workBuilding == this)
                AddCurrentWorker(resident);
        }
    }

    public virtual void ExitBuilding(Entity entity)
    {
        entities.Remove(entity);
    }

    public void AddWorker(Resident worker)
    {
        workers.Add(worker);
    }

    public void RemoveWorker(Resident worker)
	{
        workers.RemoveAt(worker.workerIndex);

        for (int i = 0; i < workers.Count; i++)
        {
            workers[i].SetWorkerIndex(i);
        }
    }

    public void AddCurrentWorker(Resident worker)
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

    protected void InvokeStartConstructing(Building building)
    {
        onAnyBuildingStartConstructing?.Invoke(building);
        onBuildingStartConstructing?.Invoke();
    }

    protected void InvokeFinishConstructing(Building building)
    {
        onAnyBuildingFinishConstructing?.Invoke(building);
        onBuildingFinishConstructing?.Invoke();
    }

    public int GetFloorIndex()
    {
        if (buildingPlace)
        {
            return buildingPlace.floorIndex;
        }
        else
        {
            return 0;
        }
    }

    public int GetPlaceIndex()
    {
        if (buildingPlace)
        {
            return buildingPlace.buildingPlaceIndex;
        }
        else
        {
            return 0;
        }
    }
}
