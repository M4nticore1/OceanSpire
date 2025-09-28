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
	[HideInInspector] public int workersCount { get; private set; } = 0;
    public List<Entity> entities { get; private set; } = new List<Entity>();
    public List<Resident> workers { get; private set; } = new List<Resident>();

    [Header("Data")]
    public BuildingData buildingData = null;

    public BuildingLevelData[] buildingLevelsData = { };

    public BuildingPlace buildingPlace = null;

    //[HideInInspector] public int floorIndex { get; private set; } = 0;
    //[HideInInspector] public int buildingPlace.buildingPlaceIndex { get; private set; } = 0;

    [Header("Construction")]
    public BuildingConstruction spawnedBuildingConstruction = null;
    public bool isRuined = false;

    public static event Action<Building> OnBuildingPlaced;
    public static event Action<Building> OnBuildingUpgraded;

    protected virtual void Start()
    {

	}

    protected virtual void Update()
    {

    }

    public virtual void Place(BuildingPlace buildingPlace)
    {
        this.buildingPlace = buildingPlace;
        gameManager = FindAnyObjectByType<GameManager>();
		cityManager = FindAnyObjectByType<CityManager>();

        //floorIndex = buildingPlace.floorIndex;
        //buildingPlace.buildingPlaceIndex = buildingPlace.buildingPlaceIndex;

        //Debug.Log(buildingPlace.floorIndex + buildingData.buildingName + buildingPlace.floorIndex + " " + buildingPlace.buildingPlaceIndex);

        StorageBuildingComponent storageBuilding = GetComponent<StorageBuildingComponent>();
        //TowerGateBuilding towerGateComponent = GetComponent<TowerGateBuilding>();

        if (storageBuilding)
		    storageBuilding.Build();

		UpdateBuildingConstruction();

        if (GetType() == typeof(Building))
            InvokeBuildingPlaced(this);
	}

    protected virtual void UpdateBuildingConstruction()
    {

    }

	protected virtual void BuildConstruction()
	{
		if (spawnedBuildingConstruction)
        {
            Destroy(spawnedBuildingConstruction.gameObject);
        }
    }

    public virtual void Upgrade()
    {
        if (isRuined)
            isRuined = false;
        else
            levelIndex++;

        OnBuildingUpgraded?.Invoke(this);

        UpdateBuildingConstruction();
        //BuildConstruction();
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

        OnBuildingUpgraded?.Invoke(this);
        buildingPlace.DestroyBuilding();
        Destroy(gameObject);
    }

    public virtual void EnterBuilding(Entity entity)
    {
        entities.Add(entity);
    }

    public virtual void ExitBuilding(Entity entity)
    {
        entities.Remove(entity);
    }

    public void AddWorker(Resident worker)
    {
        workersCount++;
        workers.Add(worker);
    }

    public void RemoveWorker(Resident worker)
	{
        workers.RemoveAt(worker.workerIndex);
        workersCount--;

        for (int i = 0; i < workersCount; i++)
        {
            workers[i].SetWorkerIndex(i);
        }
    }

    protected void InvokeBuildingPlaced(Building building)
    {
        OnBuildingPlaced?.Invoke(building);
    }

    protected void InvokeBuildingUpgraded(Building building)
    {
        OnBuildingUpgraded?.Invoke(building);
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

    public int GetBuildingPlaceIndex()
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
