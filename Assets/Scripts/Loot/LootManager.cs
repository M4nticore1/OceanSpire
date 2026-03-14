using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;

    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private float[] currentSpawnContainersTime;
    private float[] currentTimeToSpawnContainers;

    // Spawn Time
    private float lastSpawnTime = 0f;
    private const float spawnFrequency = 0.5f;

    // Update Time
    private float lastUpdateFrequency = 0f;
    private const float updateFrequency = 0.05f;
    public const float spawnDistance = 150.0f;

    // Spawn Position
    private const float spawnMaxOffsetYaw = 60.0f;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        SpawningLootContainers();
        UpdateLootContainers();
    }

    private void Initialize()
    {
        LootContainer[] lootContainer = LootContainersList.Instance.lootContainers;
        int count = lootContainer.Length;
        currentTimeToSpawnContainers = new float[count];
        currentSpawnContainersTime = new float[count];

        for (int i = 0; i < lootContainer.Length; i++) {
            float spawnTime = Random.Range(lootContainer[i].spawnMinTime, lootContainer[i].spawnMaxTime);
            currentTimeToSpawnContainers[i] = spawnTime;
        }
    }

    private void SpawnLootContainer(LootContainer container, int index)
    {
        if (container.FloorsCountToSpawn > buildingsManager.BuiltFloors.Count)
            return;

        currentSpawnContainersTime[index] += spawnFrequency;
        if (currentSpawnContainersTime[index] < currentTimeToSpawnContainers[index])
            return;

        Vector3 windDir = WindManager.Instance.windDirection;
        Vector2 baseDir = new Vector2(windDir.x, windDir.z).normalized;

        float rotationOffsetYaw = Random.Range(-spawnMaxOffsetYaw / 2f, spawnMaxOffsetYaw / 2f);
        float radians = rotationOffsetYaw * Mathf.Deg2Rad;
        Vector2 rotatedDir = new Vector2(
            baseDir.x * Mathf.Cos(radians) - baseDir.y * Mathf.Sin(radians),
            baseDir.x * Mathf.Sin(radians) + baseDir.y * Mathf.Cos(radians)
        );

        int maxFloorNumber = container.maxSpawnFloorNumber > 0 ? container.maxSpawnFloorNumber : container.minSpawnFloorNumber > 0 ? (buildingsManager.BuiltFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;

        float spawnFloorNumber = Random.Range((float)container.minSpawnFloorNumber, maxFloorNumber);
        float positionY = spawnFloorNumber * BuildingsManager.FloorHeight;

        Vector3 spawnPosition = new Vector3(
            -rotatedDir.x * spawnDistance,
            positionY,
            -rotatedDir.y * spawnDistance
        );

        float rotationAngle = Random.Range(0f, 360f);
        Quaternion spawnRotation = Quaternion.Euler(0f, rotationAngle, 0f);

        LootContainer lootContainer = Instantiate(container, spawnPosition, spawnRotation);
        lootContainer.Init((int)spawnFloorNumber);
        spawnedLootContainers.Add(lootContainer);

        currentTimeToSpawnContainers[index] = Random.Range(container.spawnMinTime, container.spawnMaxTime);
        currentSpawnContainersTime[index] = 0f;
    }

    private void SpawningLootContainers()
    {
        if (Time.time < lastSpawnTime + spawnFrequency)
            return;

        for (int i = 0; i < LootContainersList.Instance.lootContainers.Length; i++) {
            SpawnLootContainer(LootContainersList.Instance.lootContainers[i], i);
        }
        lastSpawnTime = Time.time;
    }

    private void UpdateLootContainers()
    {
        if (Time.time < lastUpdateFrequency + updateFrequency)
            return;

        for (int i = spawnedLootContainers.Count - 1; i >= 0; i--) {
            var container = spawnedLootContainers[i];
            if (container)
                container.Tick(Time.deltaTime / updateFrequency);
            else
                spawnedLootContainers.RemoveAt(i);
        }
        lastUpdateFrequency = Time.time;
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