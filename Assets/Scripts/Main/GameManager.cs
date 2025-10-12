using System.Collections.Generic;
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
    public static CityManager cityManager;

    [HideInInspector] public float worldTime = 0.0f;

    [Header("Buildings")]
    public List<Building> buildingPrefabs = new List<Building>();
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
    private const float lootContainersSpawnOffsetYaw = 30.0f;

    private float checkLootContainersPositionTime = 0.0f;
    private const float checkLootContainersPositionRate = 2.0f;

    [Header("NPC")]
    public Resident residentPrefab = null;

    // Wind
    [HideInInspector] public Vector2 windDirection = Vector2.zero;
    [HideInInspector] public float windRotation = 0;
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

    private void Awake()
    {
        cityManager = FindAnyObjectByType<CityManager>();
    }

    private void Start()
    {
        SetLootContainersSpawnTime();

        ChangeWind();
        windSpeed = newWindSpeed;
        windDirection = newWindDirection;

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

        ChangingWind();
        SpawningLootContainers();
    }

    private void SpawningLootContainers()
    {
        currentTimeToSpawnLootContainer += Time.deltaTime;

        if (currentTimeToSpawnLootContainer >= lootContainerSpawnFrequency)
        {
            for (int i = 0; i < lootContainerPrefabs.Count; i++)
            {
                spawnedLootContainersTime[i] += currentTimeToSpawnLootContainer;

                if (cityManager.builtFloors.Count >= lootContainerPrefabs[i].floorsCountToSpawn && spawnedLootContainersTime[i] >= lootContainerPrefabs[i].spawnTime)
                {
                    float rotationOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnOffsetYaw, lootContainersSpawnOffsetYaw);
                    Quaternion windRotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
                    Vector3 windDirection = windRotation * new Vector3(this.windDirection.x, 0, this.windDirection.y);
                    Vector2 normalizedWindDirection = new Vector2(windDirection.x, windDirection.z).normalized;

                    // Обход острова
                    //float moveRotationOffset = (rotationOffsetYaw > 0 ? -10f : 10f) - (rotationOffsetYaw);
                    //Quaternion moveRotation = Quaternion.Euler(0, rotationOffsetYaw + moveRotationOffset, 0);
                    //Vector3 moveDirection3D = moveRotation * new Vector3(this.windDirection.x, 0, this.windDirection.y);
                    //Vector3 normalizedMoveDirection = moveDirection3D.normalized;

                    // Spawn position
                    Vector3 rangePosition = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
                    int maxFloorNumber = lootContainerPrefabs[i].maxSpawnFloorNumber > 0 ? lootContainerPrefabs[i].maxSpawnFloorNumber : lootContainerPrefabs[i].minSpawnFloorNumber > 0 ? (cityManager.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
                    float spawnPositionY = UnityEngine.Random.Range((float)lootContainerPrefabs[i].minSpawnFloorNumber, maxFloorNumber) * CityManager.floorHeight;
                    Vector3 spawnPosition = new Vector3(-normalizedWindDirection.x * lootContainersSpawnDistance, spawnPositionY, -normalizedWindDirection.y * lootContainersSpawnDistance);

                    // Spawn rotation
                    float angle = UnityEngine.Random.Range(0, 360);
                    Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

                    LootContainer lootContainer = Instantiate(lootContainerPrefabs[i], spawnPosition, spawnRotation);
                    lootContainer.moveDirection = new Vector3(this.windDirection.x, 0, this.windDirection.y).normalized;

                    lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
                    spawnedLootContainersTime[i] = 0;
                }
            }

            currentTimeToSpawnLootContainer = 0;
        }
    }

    private void SetLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++)
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

        for (int i = 0; i < itemsData.Count; i++)
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

        for (int i = 0; i < itemsData.Count; i++)
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
