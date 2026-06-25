using System.Collections.Generic;
using UnityEngine;

public class DriftingLootManager : MonoBehaviour
{
    public static DriftingLootManager Instance;

    [SerializeField] private LootContainersList driftingLootList;
    [SerializeField] private BuildingsManager buildingsManager;

    public List<SwimmingDriftingLoot> SpawnedSwimmingDriftingLoot = new();
    public List<FlyingDriftingLoot> SpawnedFlyingDriftingLoot = new();

    public float[] CurrentSpawnTime { get; private set; }
    public float[] NextSpawnTime { get; private set; }

    // Spawn Time
    private float lastSpawnTime = 0f;
    private const float spawnFrequency = 0.5f;

    // Update Time
    [SerializeField] private float updatePositionFrequency = 0.05f;
    private float currentUpdatePositionTime = 0f;

    private const float spawnMaxOffsetYaw = 60.0f;

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

    public void Init(DriftingLootSystemData driftingLootData)
    {
        var lootContainer = driftingLootList.LootContainers;
        int count = lootContainer.Length;
        NextSpawnTime = new float[count];
        CurrentSpawnTime = new float[count];

        if (driftingLootData != null) {
            SetSpawnTime(driftingLootData.NextSpawnTime);
            SetCurrentSpawnTime(driftingLootData.CurrentSpawnTime);
            SpawnDriftingLoot(driftingLootData.SwimmingDriftingLoot);
            SpawnDriftingLoot(driftingLootData.FlyingDriftingLoot);
        }
        else {
            for (int i = 0; i < lootContainer.Length; i++) {
                float spawnTime = UnityEngine.Random.Range(lootContainer[i].Definition.MinSpawnTime, lootContainer[i].Definition.MaxSpawnTime);
                NextSpawnTime[i] = spawnTime;
            }
        }
    }

    public void RegisterSwimmingDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        SpawnedSwimmingDriftingLoot.Add(driftingLoot);
    }

    public void UnregisterSwimmingDriftingLoot(SwimmingDriftingLoot driftingLoot)
    {
        if (!SpawnedSwimmingDriftingLoot.Contains(driftingLoot)) {
            Debug.Log($"SpawnedSwimmingDriftingLoot is already does not contain {driftingLoot}");
            return;
        }

        SpawnedSwimmingDriftingLoot.Remove(driftingLoot);
    }

    public void RegisterFlyingDriftingLoot(FlyingDriftingLoot driftingLoot)
    {
        SpawnedFlyingDriftingLoot.Add(driftingLoot);
    }

    public void UnregisterFlyingDriftingLoot(FlyingDriftingLoot driftingLoot)
    {
        if (!SpawnedFlyingDriftingLoot.Contains(driftingLoot)) {
            Debug.Log($"SpawnedFlyingDriftingLoot is already does not contain {driftingLoot}");
            return;
        }

        SpawnedFlyingDriftingLoot.Remove(driftingLoot);
    }

    private void SetSpawnTime(float[] values)
    {
        if (values == null) {
            Debug.Log($"Spawn Time array not fount at {name}");
            return;
        }

        for (int i = 0; i < values.Length; i++) {
            if (NextSpawnTime.Length <= i) break;
            float value = values[i];

            NextSpawnTime[i] = value;
        }
    }

    private void SetCurrentSpawnTime(float[] values)
    {
        if (values == null) {
            Debug.Log($"Current Spawn Time array not fount at {name}");
            return;
        }

        for (int i = 0; i < values.Length; i++) {
            if (CurrentSpawnTime.Length <= i) break;
            float value = values[i];

            CurrentSpawnTime[i] = value;
        }
    }

    private void SpawnDriftingLoot(DriftingLootData[] driftingLootData)
    {
        if (driftingLootData == null) {
            Debug.Log($"driftingLootData not fount at {name}");
            return;
        }

        foreach (var data in driftingLootData) {
            var prefab = driftingLootList.GetDriftingLoot(data.Id);

            DriftingLootFactory.CreateDriftingLoot(prefab, data);
        }
    }

    private void AddSpawnTime(int id)
    {
        CurrentSpawnTime[id] += spawnFrequency;
    }

    private void UpdateNextSpawnTime(DriftingLoot driftingLootPrefab, int index)
    {
        NextSpawnTime[index] = UnityEngine.Random.Range(driftingLootPrefab.Definition.MinSpawnTime, driftingLootPrefab.Definition.MaxSpawnTime);
        CurrentSpawnTime[index] = 0f;
    }

    private bool TrySpawnLootContainer(int id)
    {
        var prefab = driftingLootList.GetDriftingLoot(id);

        if (!ShouldSpawnLootContainer(prefab, id)) return false;

        var windDir = WindManager.Instance.WindDirection;
        var baseDir = new Vector2(windDir.x, windDir.z).normalized;

        float rotationOffsetYaw = UnityEngine.Random.Range(-spawnMaxOffsetYaw / 2f, spawnMaxOffsetYaw / 2f);
        float radians = rotationOffsetYaw * Mathf.Deg2Rad;

        var rotatedDir = new Vector2(
            baseDir.x * Mathf.Cos(radians) - baseDir.y * Mathf.Sin(radians),
            baseDir.x * Mathf.Sin(radians) + baseDir.y * Mathf.Cos(radians)
        );

        var flyingDriftingLootPrefab = prefab as FlyingDriftingLoot;
        float positionY = 0;

        if (flyingDriftingLootPrefab) {
            int minFloorNumber = flyingDriftingLootPrefab.FlyingDefinition.MinSpawnFloor;
            int maxFloorNumber = Mathf.Max(minFloorNumber, flyingDriftingLootPrefab.FlyingDefinition.MaxSpawnFloor > 0 ? flyingDriftingLootPrefab.FlyingDefinition.MaxSpawnFloor : flyingDriftingLootPrefab.FlyingDefinition.MinSpawnFloor > 0 ? buildingsManager.BuiltFloors.Count : 0);

            float spawnFloorNumber = UnityEngine.Random.Range((float)minFloorNumber, maxFloorNumber);
            positionY = spawnFloorNumber * BuildingsManager.FloorHeight + BuildingsManager.FirstFloorHeight;
        }

        var spawnPosition = new Vector3(-rotatedDir.x * WorldUtils.SpawnDistance, positionY, -rotatedDir.y * WorldUtils.SpawnDistance);

        float rotationAngle = UnityEngine.Random.Range(0f, 360f);
        var spawnRotation = Quaternion.Euler(0f, rotationAngle, 0f);

        var driftingLootData = prefab.CreateRandomData();
        driftingLootData.Position = new Vector3Data(spawnPosition);
        driftingLootData.Rotation = new Vector3Data(Vector3.zero);
        driftingLootData.MeshRotation = new Vector3Data(spawnRotation.eulerAngles);

        var driftingLoot = DriftingLootFactory.CreateDriftingLoot(prefab, driftingLootData);

        return true;
    }

    private void SpawningLootContainers()
    {
        if (Time.time < lastSpawnTime + spawnFrequency)
            return;

        for (int i = 0; i < driftingLootList.LootContainers.Length; i++) {
            AddSpawnTime(i);

            if (!TrySpawnLootContainer(i)) continue;

            UpdateNextSpawnTime(driftingLootList.LootContainers[i], i);
        }

        lastSpawnTime = Time.time;
    }

    private void UpdateLootContainers()
    {
        currentUpdatePositionTime += Time.deltaTime;

        while (currentUpdatePositionTime >= updatePositionFrequency) {
            TickDriftingLoot(SpawnedSwimmingDriftingLoot.ToArray());
            TickDriftingLoot(SpawnedFlyingDriftingLoot.ToArray());

            currentUpdatePositionTime -= updatePositionFrequency;
        }
    }

    private void TickDriftingLoot(DriftingLoot[] driftingLoot)
    {
        foreach (var loot in driftingLoot) {
            loot.Tick(Time.deltaTime / updatePositionFrequency);
        }
    }

    private bool ShouldSpawnLootContainer(DriftingLoot driftingLootPrefab, int index)
    {
        if (CurrentSpawnTime[index] < NextSpawnTime[index])
            return false;

        var flyingLootPrefab = driftingLootPrefab as FlyingDriftingLoot;
        if (flyingLootPrefab) {
            if (flyingLootPrefab.FlyingDefinition.FloorsToSpawn > buildingsManager.BuiltFloors.Count) return false;
        }

        return true;
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