using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private float[] currentSpawnContainersTime;
    private float[] currentTimeToSpawnContainers;

    // Spawn Time
    private float lastSpawnTime = 0f;
    private const float spawnFrequency = 0.5f;

    // Update Time
    private float lastUpdateFrequency = 0f;
    private const float updateFrequency = 0.05f;
    public const float spawnDistance = 250.0f;

    // Spawn Position
    private const float spawnMaxOffsetYaw = 30.0f;

    private void Awake()
    {
        Instance = this;
    }

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
        if (container.FloorsCountToSpawn > CityManager.Instance.builtFloors.Count) return;

        currentSpawnContainersTime[index] += spawnFrequency;

        if (currentSpawnContainersTime[index] < currentTimeToSpawnContainers[index]) return;

        float rotationOffsetYaw = Random.Range(-spawnMaxOffsetYaw, spawnMaxOffsetYaw);
        Quaternion rotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
        Vector3 direction = rotation * WindManager.Instance.windDirection;
        //Vector2 normalizedDirection = new Vector2(direction.x, direction.z).normalized;
        //Vector2 windDorection = CityManager.Instance.windDirection.normalized;

        // Spawn position
        int maxFloorNumber = container.maxSpawnFloorNumber > 0 ? container.maxSpawnFloorNumber : container.minSpawnFloorNumber > 0 ? (CityManager.Instance.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
        float spawnFloorNumber = Random.Range((float)container.minSpawnFloorNumber, maxFloorNumber);
        float positionY = spawnFloorNumber * CityManager.floorHeight;
        Vector3 position = -direction * spawnDistance;
        Vector3 spawnPosition = new Vector3(position.x, positionY, position.z);

        // Spawn rotation
        float rotationAngle = UnityEngine.Random.Range(0, 360);
        Quaternion spawnRotation = Quaternion.Euler(0, rotationAngle, 0);

        LootContainer lootContainer = Object.Instantiate(container, spawnPosition, spawnRotation);
        lootContainer.InitializeContainer((int)spawnFloorNumber);
        spawnedLootContainers.Add(lootContainer);

        currentTimeToSpawnContainers[index] = Random.Range(container.spawnMinTime, container.spawnMaxTime);
        currentSpawnContainersTime[index] = 0;
    }

    private void SpawningLootContainers()
    {
        if (Time.time < lastSpawnTime + spawnFrequency) return;

        for (int i = 0; i < LootContainersList.Instance.lootContainers.Length; i++) {
            SpawnLootContainer(LootContainersList.Instance.lootContainers[i], i);
        }
        lastSpawnTime = Time.time;
    }

    private void UpdateLootContainers()
    {
        if (Time.time < lastUpdateFrequency + updateFrequency) return;

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
