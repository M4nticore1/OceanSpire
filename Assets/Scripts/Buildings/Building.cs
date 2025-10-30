using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static BuildingData;

public enum BuildingPosition
{
    Straight,
    Corner
}

[AddComponentMenu("")]
public class Building : MonoBehaviour
{
    [HideInInspector] protected GameManager gameManager = null;
    [HideInInspector] public CityManager cityManager = null;
    [HideInInspector] public bool isInitialized = false;
    [HideInInspector] protected bool isSelected = false;

    [HideInInspector] public int levelIndex { get; private set; } = 0;
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Resident> workers { get; private set; } = new List<Resident>();
    public List<Resident> currentWorkers { get; private set; } = new List<Resident>();

    [Header("Data")]
    public BuildingData buildingData = null;
    public BuildingLevelData[] buildingLevelsData = { };

    [HideInInspector] public BuildingPlace buildingPlace = null;

    [Header("Construction")]
    public bool isRuined = false;
    [HideInInspector] public bool isUnderConstruction { get; private set; } = false;
    private List<int> currentConstructingResourceAmount = new List<int>();
    [HideInInspector] public BuildingConstruction spawnedBuildingConstruction = null;

    [HideInInspector] private GameObject spawedBuildingInterior = null;
    [HideInInspector] public int interiorIndex = 0;

    public static event Action<Building> onAnyBuildingStartConstructing;
    public event Action onBuildingStartConstructing;
    public static event Action<Building> onAnyBuildingFinishConstructing;
    public event Action onBuildingFinishConstructing;

    public static event Action<Building> onAnyBuildingDemolished;
    public event Action onBuildingDemolished;

    [HideInInspector] public StorageBuildingComponent storageComponent = null;
    [HideInInspector] public ProductionBuildingComponent productionComponent = null;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
    }

    protected virtual void Update()
    {

    }

    // Constructing
    public virtual void InitializeBuilding(BuildingPlace buildingPlace)
    {
        this.buildingPlace = buildingPlace;
        isInitialized = true;

        storageComponent = GetComponent<StorageBuildingComponent>();
        productionComponent = GetComponent<ProductionBuildingComponent>();
    }

    public virtual void Place(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    {
        InitializeBuilding(buildingPlace);

        this.levelIndex = levelIndex;
        isUnderConstruction = requiresConstruction;
        this.interiorIndex = interiorIndex;

        if (isUnderConstruction)
            StartBuilding(levelIndex);
        else
            Build(levelIndex, interiorIndex);
    }

    private IEnumerator PlaceCoroutine(BuildingPlace buildingPlace, int levelIndex, bool requiresConstruction, int interiorIndex)
    {
        yield return new WaitForEndOfFrame();

        InitializeBuilding(buildingPlace);

        if (isUnderConstruction)
            StartBuilding(levelIndex);
        else
            Build(levelIndex, interiorIndex);

        this.levelIndex = levelIndex;
        isUnderConstruction = requiresConstruction;
        this.interiorIndex = interiorIndex;
    }

    public void StartBuilding(int nextLevelIndex)
    {
        isUnderConstruction = true;

        UpdateBuildingConstruction(nextLevelIndex);

        onAnyBuildingStartConstructing?.Invoke(this);
        onBuildingStartConstructing?.Invoke();
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

    private void Build(int newLevelIndex, int interiorIndex)
    {
        //StorageBuildingComponent storageBuilding = GetComponent<StorageBuildingComponent>();
        //ProductionBuildingComponent productionBuildingComponent = GetComponent<ProductionBuildingComponent>();

        if (storageComponent)
            storageComponent.Build();
        if (productionComponent)
            productionComponent.Build();

        UpdateBuildingConstruction(levelIndex);

        if (spawnedBuildingConstruction && spawnedBuildingConstruction.buildingInteriors.Count > 0)
        {
            if (interiorIndex < 0)
                interiorIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingInteriors.Count);

            spawedBuildingInterior = Instantiate(spawnedBuildingConstruction.buildingInteriors[interiorIndex], transform);
        }

        onAnyBuildingFinishConstructing?.Invoke(this);
        onBuildingFinishConstructing?.Invoke();
        //InvokeFinishConstructing(this);
    }

    public virtual void Demolish()
    {
        List<ItemEntry> resourceToBuilds = buildingLevelsData[levelIndex].resourcesToBuild;

        for (int i = 0; i < resourceToBuilds.Count; i++)
        {
            int itemIndex = GameManager.GetItemIndexById(gameManager.itemsData, (int)resourceToBuilds[i].itemData.itemId);
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
            Destroy(spawedBuildingInterior);
        }
    }

    public void AddConstructingResources(ItemEntry item)
    {
        List<ItemData> itemsData = new List<ItemData>();
        for (int i = 0; i < buildingLevelsData[levelIndex].resourcesToBuild.Count; i++)
        {
            itemsData.Add(buildingLevelsData[levelIndex].resourcesToBuild[i].itemData);
        }
        int index = GameManager.GetItemIndexById(itemsData, (int)item.itemData.itemId);

        currentConstructingResourceAmount[index] += item.amount;
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

    public void AddWorker(Resident worker)
    {
        workers.Add(worker);
        worker.SetWorkerIndex(workers.Count);
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

    public void Select()
    {
        isSelected = true;

        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Outlined");
        }
    }

    public void Deselect()
    {
        isSelected = false;

        foreach (GameObject child in GameUtils.GetAllChildren(transform))
        {
            child.layer = LayerMask.NameToLayer("Default");
        }
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

    public Vector3 GetInteractionPointPosition()
    {
        if (spawnedBuildingConstruction.buildingInteractions.Count > 0 && spawnedBuildingConstruction.buildingInteractions[0].waypoints.Count > 0)
            return spawnedBuildingConstruction.buildingInteractions[0].waypoints[0].position;
        else
            return transform.position;
    }

    public Vector3 GetPickupItemPointPosition()
    {
        if (spawnedBuildingConstruction.pickupItemPoints.Count > 0)
            return spawnedBuildingConstruction.pickupItemPoints[0].position;
        else
            return transform.position;
    }
}
