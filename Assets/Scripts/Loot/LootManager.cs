using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LootManager
{
    private static LootManager _instance = null;
    public static LootManager Instance => _instance ??= new LootManager();

    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private List<float> currentSpawnContainersTime = new List<float>();
    private List<float> currentTimeToSpawnContainers = new List<float>();
    private const float lootContainerSpawnFrequency = 0.5f;
    private const float updateLootFrequency = 0.05f;
    public const float spawnDistance = 250.0f;
    private const float spawnMaxOffsetYaw = 30.0f;

    public LootManager()
    {
        Run();
    }

    private async void Run()
    {
        CreateLootContainersSpawnTime();
        await UniTask.WhenAll(SpawningLootContainers(), UpdateLootContainers());
    }

    private void SpawnLootContainer(LootContainer container, int index)
    {
        if (container.FloorsCountToSpawn > GameManager.Instance.builtFloors.Count) return;

        currentSpawnContainersTime[index] += lootContainerSpawnFrequency;

        if (currentSpawnContainersTime[index] < currentTimeToSpawnContainers[index]) return;

        float rotationOffsetYaw = Random.Range(-spawnMaxOffsetYaw, spawnMaxOffsetYaw);
        Quaternion rotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
        Vector3 direction = rotation * new Vector3(GameManager.Instance.windDirection.x, 0, GameManager.Instance.windDirection.y);
        Vector2 normalizedDirection = new Vector2(direction.x, direction.z).normalized;
        Vector2 windDorection = GameManager.Instance.windDirection.normalized;

        // Spawn position
        Vector3 rangePosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized;
        int maxFloorNumber = container.maxSpawnFloorNumber > 0 ? container.maxSpawnFloorNumber : container.minSpawnFloorNumber > 0 ? (GameManager.Instance.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
        float spawnFloorNumber = Random.Range((float)container.minSpawnFloorNumber, maxFloorNumber);
        float positionY = spawnFloorNumber * GameManager.floorHeight;
        float positionX = (-windDorection.x * spawnDistance) - (normalizedDirection.x * spawnDistance) + (GameManager.Instance.windDirection.x * spawnDistance);
        float positionZ = (-windDorection.x * spawnDistance) - (normalizedDirection.y * spawnDistance) + (GameManager.Instance.windDirection.y * spawnDistance);
        Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

        // Spawn rotation
        float angle = UnityEngine.Random.Range(0, 360);
        Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

        LootContainer lootContainer = Object.Instantiate(container, spawnPosition, spawnRotation);
        lootContainer.InitializeContainer(GameManager.Instance, (int)spawnFloorNumber);
        spawnedLootContainers.Add(lootContainer);

        currentTimeToSpawnContainers[index] = Random.Range(container.spawnMinTime, container.spawnMaxTime);
        currentSpawnContainersTime[index] = 0;
    }

    private async UniTask SpawningLootContainers()
    {
        while (true) {
            for (int i = 0; i < GameManager.Instance.lootContainersList.lootContainers.Length; i++) {
                SpawnLootContainer(GameManager.Instance.lootContainersList.lootContainers[i], i);
            }
            await UniTask.Delay(System.TimeSpan.FromSeconds(lootContainerSpawnFrequency));
        }
    }

    private async UniTask UpdateLootContainers()
    {
        while (true) {
            int count = 0;
            int maxCount = 20;

            for (int i = spawnedLootContainers.Count - 1; i >= 0; i--) {
                var container = spawnedLootContainers[i];
                if (container)
                    container.Tick(updateLootFrequency);
                else
                    spawnedLootContainers.RemoveAt(i);

                count++;
                if (count >= maxCount) {
                    count = 0;
                    await UniTask.Yield(); // отдать кадр
                }
            }

            await UniTask.Delay(System.TimeSpan.FromSeconds(updateLootFrequency));
        }
    }

    private void CreateLootContainersSpawnTime()
    {
        LootContainer[] lootContainer = GameManager.Instance.lootContainersList.lootContainers;
        for (int i = 0; i < lootContainer.Length; i++) {
            float spawnTime = Random.Range(lootContainer[i].spawnMinTime, lootContainer[i].spawnMaxTime);
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
