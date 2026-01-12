using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    GameManager gameManager = null;
    CityManager cityManager = null;

    [SerializeField] private List<LootContainer> lootContainerPrefabs = new List<LootContainer>();
    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private List<float> currentSpawnContainersTime = new List<float>();
    private List<float> currentTimeToSpawnContainers = new List<float>();
    private const float lootContainerSpawnFrequency = 0.5f;
    private const float updateLootFrequency = 0.05f;
    public const float spawnDistance = 250.0f;
    private const float spawnMaxOffsetYaw = 30.0f;

    private float spawnMaxTime = 0;
    private const float initialSpawnChanceMultiplier = 0.99f;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        cityManager = FindAnyObjectByType<CityManager>();
    }

    private void Start()
    {
        CreateLootContainersSpawnTime();
        StartCoroutine(SpawningLootContainersCoroutine());
        StartCoroutine(UpdateLootContainersCoroutine());
    }

    public void Initialize()
    {
        //SetLootContainersSpawnTime();
        //SpawnInitialLoot();
    }

    private void Update()
    {
        //SpawningLootContainers();
        //UpdateLootContainers();
    }

    private void SpawningLootContainers()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++) {
            SpawnLootContainer(i);
        }
    }

    private void SpawnLootContainer(int index, float wayPositionAlpha = 0)
    {
        currentSpawnContainersTime[index] += lootContainerSpawnFrequency;

        if (cityManager.builtFloors.Count >= lootContainerPrefabs[index].FloorsCountToSpawn && currentSpawnContainersTime[index] >= currentTimeToSpawnContainers[index]) {
            float rotationOffsetYaw = UnityEngine.Random.Range(-spawnMaxOffsetYaw, spawnMaxOffsetYaw);
            Quaternion rotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
            Vector3 direction = rotation * new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y);
            Vector2 normalizedDirection = new Vector2(direction.x, direction.z).normalized;
            Vector2 windDorection = gameManager.windDirection.normalized;

            // Spawn position
            Vector3 rangePosition = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
            int maxFloorNumber = lootContainerPrefabs[index].maxSpawnFloorNumber > 0 ? lootContainerPrefabs[index].maxSpawnFloorNumber : lootContainerPrefabs[index].minSpawnFloorNumber > 0 ? (cityManager.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
            float spawnFloorNumber = UnityEngine.Random.Range((float)lootContainerPrefabs[index].minSpawnFloorNumber, (float)maxFloorNumber);
            float positionY = spawnFloorNumber * CityManager.floorHeight;
            float positionX = (math.lerp(-windDorection.x, windDorection.x, wayPositionAlpha) * spawnDistance) - (normalizedDirection.x * spawnDistance) + (gameManager.windDirection.x * spawnDistance);
            float positionZ = (math.lerp(-windDorection.y, windDorection.y, wayPositionAlpha) * spawnDistance) - (normalizedDirection.y * spawnDistance) + (gameManager.windDirection.y * spawnDistance);
            Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

            // Spawn rotation
            float angle = UnityEngine.Random.Range(0, 360);
            Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

            LootContainer lootContainer = Instantiate(lootContainerPrefabs[index], spawnPosition, spawnRotation);
            lootContainer.Initialize(gameManager.windDirection, (int)spawnFloorNumber, GameManager.windSpeed);
            spawnedLootContainers.Add(lootContainer);

            currentTimeToSpawnContainers[index] = UnityEngine.Random.Range(lootContainerPrefabs[index].spawnMinTime, lootContainerPrefabs[index].spawnMaxTime);
            currentSpawnContainersTime[index] = 0;
        }
    }

    private IEnumerator SpawningLootContainersCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(lootContainerSpawnFrequency);
            SpawningLootContainers();
        }
    }

    private void UpdateLootContainers()
    {
        for (int i = spawnedLootContainers.Count - 1; i >= 0; i--) {
            var container = spawnedLootContainers[i];
            if (container)
                container.Tick(updateLootFrequency);
            else
                spawnedLootContainers.RemoveAt(i);
        }
    }

    private IEnumerator UpdateLootContainersCoroutine()
    {
        while (true) {
            yield return new WaitForSeconds(updateLootFrequency);
            UpdateLootContainers();
        }
    }

    private void CreateLootContainersSpawnTime()
    {
        for (int i = 0; i < lootContainerPrefabs.Count; i++) {
            float spawnTime = UnityEngine.Random.Range(lootContainerPrefabs[i].spawnMinTime, lootContainerPrefabs[i].spawnMaxTime);
            currentTimeToSpawnContainers.Add(spawnTime);
            currentSpawnContainersTime.Add(0f);
        }
    }

    //private void SpawnInitialLoot()
    //{
    //    for (int i = 0; i < lootContainerPrefabs.Count; i++) {
    //        LootContainer loot = lootContainerPrefabs[i];
    //        float minTime = loot.spawnMinTime;
    //        float maxTime = loot.spawnMaxTime;
    //        float rotationOffsetYaw = UnityEngine.Random.Range(-lootContainersSpawnOffsetYaw, lootContainersSpawnOffsetYaw);

    //        //float awerage = loot.spawnMaxTime - ((loot.spawnMaxTime - loot.spawnMinTime) / 2);
    //        float spawnChange = math.lerp(minTime, maxTime, initialSpawnChanceMultiplier);

    //        //Debug.Log(spawnChange);

    //        float chance = UnityEngine.Random.Range(minTime, maxTime);
    //        //Debug.Log(chance);
    //        while (chance < spawnChange) {
    //            int side = UnityEngine.Random.Range(0, 2);
    //            float alpha = UnityEngine.Random.Range(side == 0 ? 0f : 0.6f, side == 0 ? 0.4f : 1f);
    //            SpawnLootContainer(i, alpha);
    //            chance = UnityEngine.Random.Range(minTime, maxTime);
    //        }
    //    }
    //}
}
