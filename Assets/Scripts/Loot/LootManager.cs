using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    GameManager gameManager = null;
    CityManager cityManager = null;

    [SerializeField] private List<LootContainer> lootContainerPrefabs = new List<LootContainer>();
    private List<LootContainer> spawnedLootContainers = new List<LootContainer>();
    private List<float> spawnedLootContainersTime = new List<float>();
    private float lootUpdateFrequency = 0.02f;
    private double lastUpdateTime = 0f;

    private const float lootContainerSpawnFrequency = 0.5f;
    private double lastLootContainerSpawnTime = 0;

    public const float lootContainersSpawnDistance = 250.0f;
    private const float lootContainersSpawnOffsetYaw = 30.0f;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
    }

    void Start()
    {
        SetLootContainersSpawnTime();

        for (int i = 0; i < lootContainerPrefabs.Count; i++)
            spawnedLootContainersTime.Add(0.0f);
    }

    void Update()
    {
        SpawningLootContainers();

        if (Time.realtimeSinceStartupAsDouble >= lastUpdateTime + lootUpdateFrequency)
        {
            for (int i = 0; i < spawnedLootContainers.Count; i++)
            {
                spawnedLootContainers[i].Tick((float)(Time.realtimeSinceStartupAsDouble - lastUpdateTime));
            }

            lastUpdateTime = Time.realtimeSinceStartupAsDouble;
        }
    }

    private void SpawningLootContainers()
    {
        if (Time.realtimeSinceStartupAsDouble >= lastLootContainerSpawnTime + lootContainerSpawnFrequency)
        {
            for (int i = 0; i < lootContainerPrefabs.Count; i++)
            {
                spawnedLootContainersTime[i] += (float)(Time.realtimeSinceStartupAsDouble - lastLootContainerSpawnTime);

                if (cityManager.builtFloors.Count >= lootContainerPrefabs[i].floorsCountToSpawn && spawnedLootContainersTime[i] >= lootContainerPrefabs[i].spawnTime)
                {
                    float rotationOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnOffsetYaw, lootContainersSpawnOffsetYaw);
                    Quaternion windRotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
                    Vector3 windDirection = windRotation * new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y);
                    Vector2 normalizedWindDirection = new Vector2(windDirection.x, windDirection.z).normalized;

                    // Обход острова
                    //float moveRotationOffset = (rotationOffsetYaw > 0 ? -10f : 10f) - (rotationOffsetYaw);
                    //Quaternion moveRotation = Quaternion.Euler(0, rotationOffsetYaw + moveRotationOffset, 0);
                    //Vector3 moveDirection3D = moveRotation * new Vector3(this.windDirection.x, 0, this.windDirection.y);
                    //Vector3 normalizedMoveDirection = moveDirection3D.normalized;

                    // Spawn position
                    Vector3 rangePosition = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
                    int maxFloorNumber = lootContainerPrefabs[i].maxSpawnFloorNumber > 0 ? lootContainerPrefabs[i].maxSpawnFloorNumber : lootContainerPrefabs[i].minSpawnFloorNumber > 0 ? (cityManager.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
                    float spawnPositionY = Random.Range((float)lootContainerPrefabs[i].minSpawnFloorNumber, maxFloorNumber) * CityManager.floorHeight;
                    Vector3 spawnPosition = new Vector3(-normalizedWindDirection.x * lootContainersSpawnDistance, spawnPositionY, -normalizedWindDirection.y * lootContainersSpawnDistance);

                    // Spawn rotation
                    float angle = UnityEngine.Random.Range(0, 360);
                    Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

                    LootContainer lootContainer = Instantiate(lootContainerPrefabs[i], spawnPosition, spawnRotation);
                    lootContainer.moveDirection = new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y).normalized;
                    spawnedLootContainers.Add(lootContainer);

                    lootContainerPrefabs[i].spawnTime = Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
                    spawnedLootContainersTime[i] = 0;
                }
            }

            lastLootContainerSpawnTime = Time.realtimeSinceStartupAsDouble;
        }
    }

    private void SetLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++)
        {
            lootContainerPrefabs[i].spawnTime = Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
        }
    }
}
