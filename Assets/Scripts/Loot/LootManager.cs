using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    GameManager gameManager = null;
    CityManager cityManager = null;

    [SerializeField] private List<LootContainer> lootContainerPrefabs = new List<LootContainer>();
    public List<LootContainer> spawnedLootContainers/* { get; private set; }*/ = new List<LootContainer>();
    private List<float> spawnedLootContainersTime = new List<float>();
    private float lootUpdateFrequency = 0.02f;
    private double lastUpdateTime = 0f;

    private const float lootContainerSpawnFrequency = 0.5f;
    private double lastLootContainerSpawnTime = 0;

    public const float lootContainersSpawnDistance = 250.0f;
    private const float lootContainersSpawnOffsetYaw = 30.0f;

    private float spawnMaxTime = 0;
    private const float initialSpawnChanceMultiplier = 0.99f;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
    }

    public void Initialize()
    {
        SetLootContainersSpawnTime();
        SpawnInitialLoot();
    }

    private void Update()
    {
        //SpawningLootContainers();
        //UpdateLootContainers();
    }

    private void SpawningLootContainers()
    {
        if (Time.timeAsDouble >= lastLootContainerSpawnTime + lootContainerSpawnFrequency)
        {
            for (int i = 0; i < lootContainerPrefabs.Count; i++)
            {
                SpawnLootContainer(i);
            }

            lastLootContainerSpawnTime = Time.timeAsDouble;
        }
    }

    private void UpdateLootContainers()
    {
        if (Time.timeAsDouble >= lastUpdateTime + lootUpdateFrequency)
        {
            for (int i = 0; i < spawnedLootContainers.Count; i++)
            {
                if (spawnedLootContainers[i])
                    spawnedLootContainers[i].Tick((float)(Time.timeAsDouble - lastUpdateTime));
                else
                {
                    spawnedLootContainers.RemoveAt(i);
                    i--;
                }
            }

            lastUpdateTime = Time.timeAsDouble;
        }
    }

    private void SetLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++)
        {
            lootContainerPrefabs[i].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
            spawnedLootContainersTime.Add(0);
        }
    }

    private void SpawnInitialLoot()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++)
        {
            LootContainer loot = lootContainerPrefabs[i];
            float minTime = loot.spawnMinTime;
            float maxTime = loot.spawnMaxTime;
            float rotationOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnOffsetYaw, lootContainersSpawnOffsetYaw);

            //float awerage = loot.spawnMaxTime - ((loot.spawnMaxTime - loot.spawnMinTime) / 2);
            float spawnChange = math.lerp(minTime, maxTime, initialSpawnChanceMultiplier);

            //Debug.Log(spawnChange);

            float chance = UnityEngine.Random.Range(minTime, maxTime);
            //Debug.Log(chance);
            while (chance < spawnChange)
            {
                int side = UnityEngine.Random.Range(0, 2);
                float alpha = UnityEngine.Random.Range(side == 0 ? 0f : 0.6f, side == 0 ? 0.4f : 1f);
                SpawnLootContainer(i, alpha);
                chance = UnityEngine.Random.Range(minTime, maxTime);
            }
        }
    }

    private void SpawnLootContainer(int index, float wayPositionAlpha = 0)
    {
        spawnedLootContainersTime[index] += (float)(Time.timeAsDouble - lastLootContainerSpawnTime);

        if (cityManager.builtFloors.Count >= lootContainerPrefabs[index].FloorsCountToSpawn && spawnedLootContainersTime[index] >= lootContainerPrefabs[index].spawnTime)
        {
            float rotationOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnOffsetYaw, lootContainersSpawnOffsetYaw);
            Quaternion rotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
            Vector3 direction = rotation * new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y);
            Vector2 normalizedDirection = new Vector2(direction.x, direction.z).normalized;
            Vector2 windDorection = gameManager.windDirection.normalized;

            // Spawn position
            Vector3 rangePosition = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
            int maxFloorNumber = lootContainerPrefabs[index].maxSpawnFloorNumber > 0 ? lootContainerPrefabs[index].maxSpawnFloorNumber : lootContainerPrefabs[index].minSpawnFloorNumber > 0 ? (cityManager.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
            float spawnFloorNumber = UnityEngine.Random.Range((float)lootContainerPrefabs[index].minSpawnFloorNumber, (float)maxFloorNumber);
            float positionY = spawnFloorNumber * CityManager.floorHeight;
            float positionX = (math.lerp(-windDorection.x, windDorection.x, wayPositionAlpha) * lootContainersSpawnDistance) - (normalizedDirection.x * lootContainersSpawnDistance) + (gameManager.windDirection.x * lootContainersSpawnDistance);
            float positionZ = (math.lerp(-windDorection.y, windDorection.y, wayPositionAlpha) * lootContainersSpawnDistance) - (normalizedDirection.y * lootContainersSpawnDistance) + (gameManager.windDirection.y * lootContainersSpawnDistance);
            Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

            // Spawn rotation
            float angle = UnityEngine.Random.Range(0, 360);
            Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

            LootContainer lootContainer = Instantiate(lootContainerPrefabs[index], spawnPosition, spawnRotation);
            lootContainer.Initialize(gameManager.windDirection, (int)spawnFloorNumber, GameManager.windSpeed);
            spawnedLootContainers.Add(lootContainer);

            lootContainerPrefabs[index].spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[index].spawnMinTime, lootContainerPrefabs[index].spawnMaxTime);
            spawnedLootContainersTime[index] = 0;
        }
    }
}
