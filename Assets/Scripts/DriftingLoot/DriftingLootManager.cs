using System;
using System.Collections.Generic;
using UnityEngine;

public class DriftingLootManager : MonoBehaviour
{
    public static DriftingLootManager Instance;

    [SerializeField] private LootContainersList driftingLootList;
    [SerializeField] private BuildingsManager buildingsManager;

    [SerializeField] private float spawnMaxOffsetYaw = 90.0f;
    [SerializeField] private float updatePositionFrequency = 0.05f;
    [SerializeField] private float spawnFrequency = 0.5f;

    public List<SwimmingDriftingLoot> SpawnedSwimmingDriftingLoot = new();
    public List<FlyingDriftingLoot> SpawnedFlyingDriftingLoot = new();

    public Dictionary<DriftingLootId, float> CurrentSpawnTime { get; private set; } = new();
    public Dictionary<DriftingLootId, float> NextSpawnTime { get; private set; } = new();

    private float lastSpawnTime = 0f;
    private float currentUpdatePositionTime = 0f;

    private void Awake()
    {
        if (Instance) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        SpawningLootContainers();
        UpdateLootContainers();
    }

    public void Init()
    {
        Init(DriftingLootSystemData.Default() ?? new DriftingLootSystemData());
    }

    public void Init(DriftingLootSystemData driftingLootData)
    {
        if (driftingLootList == null || driftingLootList.LootContainers == null) {
            Debug.LogError($"[{nameof(DriftingLootManager)}] LootContainersList or its containers are not assigned!");
            Init();
            return;
        }

        CurrentSpawnTime.Clear();
        NextSpawnTime.Clear();

        var lootContainers = driftingLootList.LootContainers;

        SetSpawnTime(driftingLootData.NextSpawnTime);
        SetCurrentSpawnTime(driftingLootData.CurrentSpawnTime);
        SpawnDriftingLoot(driftingLootData.SwimmingDriftingLoot);
        SpawnDriftingLoot(driftingLootData.FlyingDriftingLoot);

        foreach (var container in lootContainers) {
            if (container == null) continue;

            var id = container.Definition.Id;

            if (!NextSpawnTime.ContainsKey(id)) {
                float randomSpawnTime = UnityEngine.Random.Range(container.Definition.MinSpawnTime, container.Definition.MaxSpawnTime);
                NextSpawnTime[id] = randomSpawnTime;
            }

            if (!CurrentSpawnTime.ContainsKey(id)) {
                CurrentSpawnTime[id] = 0f;
            }
        }
    }

    public void RegisterSwimmingDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (driftingLoot != null && !SpawnedSwimmingDriftingLoot.Contains(driftingLoot)) {
            SpawnedSwimmingDriftingLoot.Add(driftingLoot);
        }
    }

    public void UnregisterSwimmingDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;
        SpawnedSwimmingDriftingLoot.Remove(driftingLoot);
    }

    public void RegisterFlyingDriftingLoot(FlyingDriftingLoot driftingLoot)
    {
        if (driftingLoot != null && !SpawnedFlyingDriftingLoot.Contains(driftingLoot)) {
            SpawnedFlyingDriftingLoot.Add(driftingLoot);
        }
    }

    public void UnregisterFlyingDriftingLoot(FlyingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;
        SpawnedFlyingDriftingLoot.Remove(driftingLoot);
    }

    public Vector3 GetSpawnPosition(DriftingLoot containerPrefab)
    {
        Vector3 windDir = WindManager.Instance != null ? WindManager.Instance.WindDirection : Vector3.forward;
        windDir = new Vector3(windDir.x, 0f, windDir.z).normalized;
        Vector3 baseSpawnPos = -windDir * WorldUtils.SpawnDistance;
        float randomYawAngle = UnityEngine.Random.Range(-spawnMaxOffsetYaw / 2, spawnMaxOffsetYaw / 2);
        Vector3 finalBaseSpawn = Quaternion.Euler(0f, randomYawAngle, 0f) * baseSpawnPos;

        float positionY = 0f;
        var flyingDriftingLootPrefab = containerPrefab as FlyingDriftingLoot;

        if (flyingDriftingLootPrefab && buildingsManager != null) {
            int minFloorNumber = flyingDriftingLootPrefab.FlyingDefinition.MinSpawnFloor;
            int maxFloorNumber = Mathf.Max(minFloorNumber, flyingDriftingLootPrefab.FlyingDefinition.MaxSpawnFloor > 0
                ? flyingDriftingLootPrefab.FlyingDefinition.MaxSpawnFloor
                : buildingsManager.BuiltFloors.Count);

            float spawnFloorNumber = UnityEngine.Random.Range((float)minFloorNumber, maxFloorNumber);
            positionY = spawnFloorNumber * BuildingsManager.FloorHeight + BuildingsManager.FirstFloorHeight;
        }

        return new Vector3(finalBaseSpawn.x, positionY, finalBaseSpawn.z);
    }

    public Vector3 GetDestinationPosition()
    {
        Vector3 windDir = WindManager.Instance != null ? WindManager.Instance.WindDirection : Vector3.forward;
        windDir = new Vector3(windDir.x, 0f, windDir.z).normalized;
        Vector3 baseDestinationPos = windDir * WorldUtils.SpawnDistance;
        float randomYawAngle = UnityEngine.Random.Range(-spawnMaxOffsetYaw / 2, spawnMaxOffsetYaw / 2);
        Vector3 finalBaseDest = Quaternion.Euler(0f, randomYawAngle, 0f) * baseDestinationPos;

        return new Vector3(finalBaseDest.x, 0f, finalBaseDest.z);
    }

    public Quaternion GetSpawnRotation()
    {
        float rotationAngle = UnityEngine.Random.Range(0f, 360f);
        return Quaternion.Euler(0f, rotationAngle, 0f);
    }

    private void SetSpawnTime(float[] values)
    {
        if (values == null) return;

        var containers = driftingLootList.LootContainers;
        for (int i = 0; i < values.Length; i++) {
            if (i >= containers.Length) break;
            if (containers[i] == null) continue;

            var id = containers[i].Definition.Id;
            NextSpawnTime[id] = values[i];
        }
    }

    private void SetCurrentSpawnTime(float[] values)
    {
        if (values == null) return;

        var containers = driftingLootList.LootContainers;
        for (int i = 0; i < values.Length; i++) {
            if (i >= containers.Length) break;
            if (containers[i] == null) continue;

            var id = containers[i].Definition.Id;
            CurrentSpawnTime[id] = values[i];
        }
    }

    private void SpawnDriftingLoot(DriftingLootData[] driftingLootData)
    {
        if (driftingLootData == null) return;

        foreach (var data in driftingLootData) {
            if (data == null) continue;

            var prefab = driftingLootList.GetDriftingLoot(data.Id);
            if (prefab != null) {
                DriftingLootFactory.CreateDriftingLoot(prefab, data);
            }
        }
    }

    private void AddSpawnTime(DriftingLootId id)
    {
        if (!CurrentSpawnTime.ContainsKey(id)) {
            CurrentSpawnTime[id] = 0f;
        }
        CurrentSpawnTime[id] += spawnFrequency;
    }

    private void UpdateNextSpawnTime(DriftingLoot driftingLootPrefab, DriftingLootId id)
    {
        NextSpawnTime[id] = UnityEngine.Random.Range(driftingLootPrefab.Definition.MinSpawnTime, driftingLootPrefab.Definition.MaxSpawnTime);
        CurrentSpawnTime[id] = 0f;
    }

    private bool TrySpawnLootContainer(DriftingLoot containerPrefab)
    {
        if (containerPrefab == null) return false;
        var id = containerPrefab.Definition.Id;

        if (!ShouldSpawnLootContainer(containerPrefab, id)) return false;

        Vector3 spawnPosition = GetSpawnPosition(containerPrefab);
        Vector3 destinationPosition = GetDestinationPosition();
        Quaternion spawnRotation = GetSpawnRotation();

        var driftingLootData = containerPrefab.CreateRandomData();
        driftingLootData.Position = new Vector3Data(spawnPosition);
        driftingLootData.Destination = new Vector3Data(destinationPosition);
        driftingLootData.Rotation = new Vector3Data(Vector3.zero);
        driftingLootData.MeshRotation = new Vector3Data(spawnRotation.eulerAngles);

        DriftingLootFactory.CreateDriftingLoot(containerPrefab, driftingLootData);
        return true;
    }

    private void SpawningLootContainers()
    {
        if (Time.time < lastSpawnTime + spawnFrequency)
            return;

        var containers = driftingLootList.LootContainers;
        for (int i = 0; i < containers.Length; i++) {
            if (containers[i] == null) continue;

            DriftingLootId id = containers[i].Definition.Id;
            AddSpawnTime(id);

            if (!TrySpawnLootContainer(containers[i])) continue;

            UpdateNextSpawnTime(containers[i], id);
        }

        lastSpawnTime = Time.time;
    }

    private void UpdateLootContainers()
    {
        currentUpdatePositionTime += Time.deltaTime;

        if (currentUpdatePositionTime > 1.0f) {
            currentUpdatePositionTime = updatePositionFrequency;
        }

        while (currentUpdatePositionTime >= updatePositionFrequency && updatePositionFrequency > 0) {
            const float tickStep = 1f;

            for (int i = SpawnedSwimmingDriftingLoot.Count - 1; i >= 0; i--) {
                if (SpawnedSwimmingDriftingLoot[i] != null) {
                    SpawnedSwimmingDriftingLoot[i].Tick(tickStep);
                }
            }

            for (int i = SpawnedFlyingDriftingLoot.Count - 1; i >= 0; i--) {
                if (SpawnedFlyingDriftingLoot[i] != null) {
                    SpawnedFlyingDriftingLoot[i].Tick(tickStep);
                }
            }

            currentUpdatePositionTime -= updatePositionFrequency;
        }
    }

    private bool ShouldSpawnLootContainer(DriftingLoot driftingLootPrefab, DriftingLootId id)
    {
        if (!CurrentSpawnTime.TryGetValue(id, out float currentTime) || !NextSpawnTime.TryGetValue(id, out float nextTime)) {
            return false;
        }

        if (currentTime < nextTime) return false;

        var flyingLootPrefab = driftingLootPrefab as FlyingDriftingLoot;
        if (flyingLootPrefab && buildingsManager != null) {
            if (flyingLootPrefab.FlyingDefinition.FloorsToSpawn > buildingsManager.BuiltFloors.Count)
                return false;
        }

        return true;
    }
}