using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;
using static BuildingData;

[System.Serializable]
public struct ResourceStack
{
    public ItemData resource;
    public int amount;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public float worldTime = 0.0f;

    // Buildings
    //[HideInInspector] public int buildedFloorsCount = 0;

    [Header("Buildings")]
    public List<Building> buildingPrefabs = new List<Building>();
    //public List<Floor> spawnedFloors = new List<Floor>();

    //private List<List<BuildingPlace>> spawnedRoomPlaces = new List<List<BuildingPlace>>();
    //[SerializeField] private List<BuildingPlace> spawnedHallPlaces = new List<BuildingPlace>();
    //private List<List<BuildingPlace>> spawnedElevatorPlaces = new List<List<BuildingPlace>>();
    //[SerializeField] private List<BuildingPlace> spawnedFloorPlaces = new List<BuildingPlace>();

    //[HideInInspector] public List<Building> allBuildings = new List<Building>();
    //[HideInInspector] public List<List<RoomBuilding>> allRooms = new List<List<RoomBuilding>>();
    //[HideInInspector] public List<List<ElevatorBuilding>> allElevators = new List<List<ElevatorBuilding>>();
    //[HideInInspector] public List<StorageBuildingComponent> spawnedStorageBuildings = new List<StorageBuildingComponent>();

    //[HideInInspector] public List<int> currentRoomsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentHallsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentElevatorsNumberOnFloor = new List<int>();
    //[HideInInspector] public List<int> currentFloorFrameNumberOnFloor = new List<int>();

    //private const float firstFloorHeight = 5.0f;
    //[HideInInspector] public const int roomsCountPerFloor = 8;
    //[HideInInspector] public const int elevatorsCountPerFloor = 12;
    //[HideInInspector] public float cityHeight = 0;

    public const float demolitionResourceRefundRate = 0.2f;

    // Items
    [Header("Items")]
    public List<ItemData> itemsData = new List<ItemData>();
    //[HideInInspector] public List<ItemInstance> items = new List<ItemInstance>();

    // Loot Containers
    private const float lootContainerSpawnFrequency = 0.1f;
    private float currentTimeToSpawnLootContainer = 0.0f;

    [Header("Loot Containers")]
    [SerializeField] private List<LootContainer> lootContainerPrefabs = new List<LootContainer>();
    private List<float> spawnedLootContainersTime = new List<float>();

    public const float lootContainersSpawnDistance = 250.0f;
    private const float lootContainersSpawnMaxOffsetYaw = 60.0f;

    private float checkLootContainersPositionTime = 0.0f;
    private const float checkLootContainersPositionRate = 2.0f;

    [Header("NPC")]
    public Resident residentPrefab = null;

    // Wind
    [HideInInspector] public Vector2 windDirection = Vector2.zero;
    private Vector2 newWindDirection = Vector2.zero;

    [HideInInspector] public float windSpeed = 0.0f;
    private float newWindSpeed = 0.0f;
    private const float windMinSpeed = 4.0f;
    private const float windMaxSppeed = 10.0f;

    private float windSpeedChangingSpeed = 0.0f;
    private const float windMinSpeedChangingSpeed = 0.05f;
    private const float windMaxSpeedChangingSpeed = 0.1f;

    private float windDirectionChangeRate = 0.0f;
    private const float windDirectionChanceMinRate = 120.0f;
    private const float windDirectionChanceMaxRate = 300.0f;
    private float windDirectionChangeTime = 0.0f;

    private float windDirectionChangeSpeed = 0.0f;
    private const float windDirectionMinChangeSpeed = 0.04f;
    private const float windDirectionMaxChangeSpeed = 0.05f;

    public const float autoSaveFrequency = 1;

    public bool hasSavedData = false;

    // Delegates
    //public delegate void StorageCapacityChangedHandler();
    //public static event System.Action OnStorageCapacityChanged;

    private void Start()
    {
        SetLootContainersSpawnTime();

        windSpeed = windMinSpeed;
        ChangeWind();

        for (int i = 0; i < lootContainerPrefabs.Count; i++)
            spawnedLootContainersTime.Add(0.0f);
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {

    }

    private void Update()
    {
        worldTime += Time.deltaTime;

        SpawningLootContainers();
        ChangingWind();
    }

    private void SpawningLootContainers()
    {
        currentTimeToSpawnLootContainer += Time.deltaTime;

        if (currentTimeToSpawnLootContainer >= lootContainerSpawnFrequency)
        {
            for (int i = 0; i < lootContainerPrefabs.Count(); i++)
            {
                spawnedLootContainersTime[i] += currentTimeToSpawnLootContainer;

                if (spawnedLootContainersTime[i] >= lootContainerPrefabs[i].spawnTime)
                {
                    float spawnPositionOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnMaxOffsetYaw / 2, lootContainersSpawnMaxOffsetYaw / 2);
                    Quaternion rotation = Quaternion.Euler(0, spawnPositionOffsetYaw, 0);
                    Vector3 direction = rotation * new Vector3(windDirection.x, 0, windDirection.y);
                    Vector2 newWindDirection = new Vector2(direction.x, direction.z).normalized;

                    //float angle = Mathf.Atan2(windDirection.y, windDirection.x) * Mathf.Rad2Deg;
                    float angle = UnityEngine.Random.Range(0, 360);
                    Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

                    Vector2 rangePosition = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
                    Vector3 spawnPosition = new Vector3(-newWindDirection.x, 0, -newWindDirection.y) * lootContainersSpawnDistance + new Vector3(rangePosition.x, 0, rangePosition.y);

                    Instantiate(lootContainerPrefabs[i], spawnPosition, spawnRotation);

                    lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
                    spawnedLootContainersTime[i] = 0;
                }
            }

            currentTimeToSpawnLootContainer = 0;
        }
    }

    private void SetLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count(); i++)
        {
            lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
        }
    }

    private void ChangingWind()
    {
        if (Time.time > windDirectionChangeTime + windDirectionChangeRate)
        {
            ChangeWind();
        }

        windDirection = math.lerp(windDirection, newWindDirection, windDirectionChangeSpeed * Time.deltaTime);
        windSpeed = math.lerp(windSpeed, newWindSpeed, windSpeedChangingSpeed * Time.deltaTime);
    }

    private void ChangeWind()
    {
        float xAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        float yAxis = UnityEngine.Random.Range(-1.0f, 1.0f);
        newWindDirection = new Vector2(xAxis, yAxis).normalized;

        windDirectionChangeRate = UnityEngine.Random.Range(windDirectionChanceMinRate, windDirectionChanceMaxRate);
        windDirectionChangeSpeed = UnityEngine.Random.Range(windDirectionMinChangeSpeed, windDirectionMaxChangeSpeed);
        newWindSpeed = UnityEngine.Random.Range(windMinSpeed, windMaxSppeed);
        windSpeedChangingSpeed = UnityEngine.Random.Range(windMinSpeedChangingSpeed, windMaxSpeedChangingSpeed);

        windDirectionChangeTime = 0;
    }

    private void CheckLootContainersPosition()
    {
        checkLootContainersPositionTime += Time.deltaTime;
    }

    public int GetItemIndexByIdName(string idName)
    {
        int id = 0;

        for (int i = 0; i < itemsData.Count(); i++)
        {
            if (itemsData[i].itemIdName == idName)
            {
                id = i;
                break;
            }
        }

        return id;
    }

    public int GetItemIndexById(int id)
    {
        int currentId = 0;

        for (int i = 0; i < itemsData.Count(); i++)
        {
            if ((int)itemsData[i].itemId == id)
            {
                currentId = i;
                break;
            }
        }

        return currentId;
    }

    public Building GetBuildingPrefabById(int buildingId)
    {
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if ((int)buildingPrefabs[i].buildingData.buildingId == buildingId)
            {
                return buildingPrefabs[i];
            }
        }

        return null;
    }

    public Building GetBuildingPrefabByIdName(string buildingIdName)
    {
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if (buildingPrefabs[i].buildingData.buildingIdName == buildingIdName)
            {
                return buildingPrefabs[i];
            }
        }

        return null;
    }
}
