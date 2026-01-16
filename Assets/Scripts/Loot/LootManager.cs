using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LootManager
{
    private GameManager gameManager = null;

    public List<LootContainer> spawnedLootContainers { get; private set; } = new List<LootContainer>();
    private List<float> currentSpawnContainersTime = new List<float>();
    private List<float> currentTimeToSpawnContainers = new List<float>();
    private const float lootContainerSpawnFrequency = 0.5f;
    private const float updateLootFrequency = 0.05f;
    public const float spawnDistance = 250.0f;
    private const float spawnMaxOffsetYaw = 30.0f;

    public LootManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public async void Run()
    {
        CreateLootContainersSpawnTime();
        await SpawningLootContainers();
        await UpdateLootContainers();
    }

    private void SpawnLootContainer(LootContainer container, int index)
    {
        if (container.FloorsCountToSpawn < gameManager.builtFloors.Count) return;

        currentSpawnContainersTime[index] += lootContainerSpawnFrequency;

        if (currentSpawnContainersTime[index] < currentTimeToSpawnContainers[index]) return;

        float rotationOffsetYaw = UnityEngine.Random.Range(-spawnMaxOffsetYaw, spawnMaxOffsetYaw);
        Quaternion rotation = Quaternion.Euler(0, rotationOffsetYaw, 0);
        Vector3 direction = rotation * new Vector3(gameManager.windDirection.x, 0, gameManager.windDirection.y);
        Vector2 normalizedDirection = new Vector2(direction.x, direction.z).normalized;
        Vector2 windDorection = gameManager.windDirection.normalized;

        // Spawn position
        Vector3 rangePosition = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0, UnityEngine.Random.Range(-1.0f, 1.0f)).normalized;
        int maxFloorNumber = container.maxSpawnFloorNumber > 0 ? container.maxSpawnFloorNumber : container.minSpawnFloorNumber > 0 ? (gameManager.builtFloors.Count + LootContainer.limitSpawnFloorsCount) : 0;
        float spawnFloorNumber = UnityEngine.Random.Range((float)container.minSpawnFloorNumber, maxFloorNumber);
        float positionY = spawnFloorNumber * GameManager.floorHeight;
        float positionX = (-windDorection.x * spawnDistance) - (normalizedDirection.x * spawnDistance) + (gameManager.windDirection.x * spawnDistance);
        float positionZ = (-windDorection.x * spawnDistance) - (normalizedDirection.y * spawnDistance) + (gameManager.windDirection.y * spawnDistance);
        Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

        // Spawn rotation
        float angle = UnityEngine.Random.Range(0, 360);
        Quaternion spawnRotation = Quaternion.Euler(0, angle, 0);

        LootContainer lootContainer = Object.Instantiate(container, spawnPosition, spawnRotation);
        lootContainer.InitializeContainer(gameManager, (int)spawnFloorNumber);
        spawnedLootContainers.Add(lootContainer);

        currentTimeToSpawnContainers[index] = UnityEngine.Random.Range(container.spawnMinTime, container.spawnMaxTime);
        currentSpawnContainersTime[index] = 0;
    }

    private async UniTask SpawningLootContainers()
    {
        while (true) {
            await UniTask.Delay(System.TimeSpan.FromSeconds(lootContainerSpawnFrequency));
            for (int i = 0; i < gameManager.lootContainersList.lootContainers.Length; i++) {
                SpawnLootContainer(gameManager.lootContainersList.lootContainers[i], i);
            }
        }
    }

    private async UniTask UpdateLootContainers()
    {
        while (true) {
            await UniTask.Delay(System.TimeSpan.FromSeconds(updateLootFrequency));
            for (int i = spawnedLootContainers.Count - 1; i >= 0; i--) {
                var container = spawnedLootContainers[i];
                if (container)
                    container.Tick(updateLootFrequency);
                else
                    spawnedLootContainers.RemoveAt(i);
            }
        }
    }

    private void CreateLootContainersSpawnTime()
    {
        LootContainer[] lootContainer = gameManager.lootContainersList.lootContainers;
        for (int i = 0; i < lootContainer.Length; i++) {
            float spawnTime = UnityEngine.Random.Range(lootContainer[i].spawnMinTime, lootContainer[i].spawnMaxTime);
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
