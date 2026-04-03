using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager instance;

    [SerializeField] private BuildingsManager buildingsManager;

    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private float[] currentSpawnContainersTime;
    private float[] currentTimeToSpawnContainers;

    // Spawn Time
    private float lastSpawnTime = 0f;
    private const float spawnFrequency = 0.5f;

    // Update Time
    [SerializeField] private float updatePositionFrequency = 0.05f;
    private float currentUpdatePositionTime = 0f;

    // Spawn Position
    public const float spawnDistance = 160.0f;
    private const float spawnMaxOffsetYaw = 60.0f;

    private void Awake()
    {
        if (instance) {
            Destroy(gameObject);
            return;
        }

        instance = this;
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

    public void RegisterLootContainer(LootContainer container)
    {
        spawnedLootContainers.Add(container);
    }

    private void Initialize()
    {
        LootContainer[] lootContainer = LootContainersList.Instance.LootContainers;
        int count = lootContainer.Length;
        currentTimeToSpawnContainers = new float[count];
        currentSpawnContainersTime = new float[count];

        for (int i = 0; i < lootContainer.Length; i++) {
            float spawnTime = Random.Range(lootContainer[i].SpawnMinTime, lootContainer[i].SpawnMaxTime);
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

        int minFloorNumber = container.MinSpawnFloorNumber;
        int maxFloorNumber = Mathf.Max(minFloorNumber, container.MaxSpawnFloorNumber > 0 ? container.MaxSpawnFloorNumber : container.MinSpawnFloorNumber > 0 ? buildingsManager.BuiltFloors.Count : 0);

        float spawnFloorNumber = Random.Range((float)minFloorNumber, maxFloorNumber);
        float firstFloorHeight = container.IsFlying ? BuildingsManager.FirstFloorHeight : 0;
        float positionY = spawnFloorNumber * BuildingsManager.FloorHeight + firstFloorHeight;

        Vector3 spawnPosition = new Vector3( -rotatedDir.x * spawnDistance, positionY, -rotatedDir.y * spawnDistance);

        float rotationAngle = Random.Range(0f, 360f);
        Quaternion spawnRotation = Quaternion.Euler(0f, rotationAngle, 0f);

        LootContainer lootContainer = Instantiate(container, spawnPosition, spawnRotation);
        lootContainer.Init((int)spawnFloorNumber);
        spawnedLootContainers.Add(lootContainer);

        currentTimeToSpawnContainers[index] = Random.Range(container.SpawnMinTime, container.SpawnMaxTime);
        currentSpawnContainersTime[index] = 0f;
    }

    private void SpawningLootContainers()
    {
        if (Time.time < lastSpawnTime + spawnFrequency)
            return;

        for (int i = 0; i < LootContainersList.Instance.LootContainers.Length; i++) {
            SpawnLootContainer(LootContainersList.Instance.LootContainers[i], i);
        }
        lastSpawnTime = Time.time;
    }

    private void UpdateLootContainers()
    {
        currentUpdatePositionTime += Time.deltaTime;

        while (currentUpdatePositionTime >= updatePositionFrequency) {
            for (int i = spawnedLootContainers.Count - 1; i >= 0; i--) {
                var container = spawnedLootContainers[i];

                if (container)
                    container.Tick(Time.deltaTime / updatePositionFrequency);
                else
                    spawnedLootContainers.RemoveAt(i);
            }

            currentUpdatePositionTime -= updatePositionFrequency;
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