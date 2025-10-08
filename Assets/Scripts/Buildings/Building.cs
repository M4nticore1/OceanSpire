using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
//using UnityEditor;
//using UnityEditor.ShaderKeywordFilter;
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

    [HideInInspector] public int levelIndex { get; private set; } = 0;
    [HideInInspector] public bool isUnderConstruction { get; private set; } = false;
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Resident> workers { get; private set; } = new List<Resident>();
    public List<Resident> currentWorkers { get; private set; } = new List<Resident>();

    [Header("Data")]
    public BuildingData buildingData = null;

    public BuildingLevelData[] buildingLevelsData = { };

    public BuildingPlace buildingPlace = null;

    //[HideInInspector] public int floorIndex { get; private set; } = 0;
    //[HideInInspector] public int buildingPlace.buildingPlaceIndex { get; private set; } = 0;

    [Header("Construction")]
    public BuildingConstruction spawnedBuildingConstruction = null;
    public bool isRuined = false;
    private GameObject spawedBuildingDetails = null;

    public static event Action<Building> OnBuildingStartConstructing;
    public static event Action<Building> OnBuildingFinishConstructing;

    protected virtual void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
    }

    protected virtual void Start()
    {

	}

    protected virtual void Update()
    {

    }

    public virtual void Place(BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction)
    {
        this.buildingPlace = buildingPlace;
        gameManager = FindAnyObjectByType<GameManager>();
		cityManager = FindAnyObjectByType<CityManager>();

        if (isUnderConstruction)
            StartBuilding(levelIndex);
        else
            Build(levelIndex);
	}

    public virtual void StartBuilding(int nextLevel)
    {
        isUnderConstruction = true;

        UpdateBuildingConstruction(nextLevel);

        if (spawnedBuildingConstruction && spawnedBuildingConstruction.buildingDetails.Count > 0)
        {
            int buildingDetailsIndex = UnityEngine.Random.Range(0, spawnedBuildingConstruction.buildingDetails.Count);
            spawedBuildingDetails = Instantiate<GameObject>(spawnedBuildingConstruction.buildingDetails[buildingDetailsIndex], transform);
        }

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

        Build(levelIndex);
    }

    public virtual void Build(int newLevelIndex)
    {
        StorageBuildingComponent storageBuilding = GetComponent<StorageBuildingComponent>();

        if (storageBuilding)
            storageBuilding.Build();

        UpdateBuildingConstruction(levelIndex);

        if (GetType() == typeof(Building))
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

        OnBuildingFinishConstructing?.Invoke(this);
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
        OnBuildingStartConstructing?.Invoke(building);
    }

    protected void InvokeFinishConstructing(Building building)
    {
        OnBuildingFinishConstructing?.Invoke(building);
    }

    public int GetFloorIndex()
    {
        if (buildingPlace)
        {
            return buildingPlace.floorIndex;
        }
        else
        {
            return -1;
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
            return -1;
        }
    }
}
