using System;
using UnityEngine;

[Serializable]
public class DriftingLootSystemData
{
    public float[] NextSpawnTime = new float[0];
    public float[] CurrentSpawnTime = new float[0];
    public SwimmingDriftingLootData[] SwimmingDriftingLoot = new SwimmingDriftingLootData[0];
    public FlyingDriftingLootData[] FlyingDriftingLoot = new FlyingDriftingLootData[0];

    public static DriftingLootSystemData Default()
    {
        return new DriftingLootSystemData();
    }

    public static DriftingLootSystemData Create(DriftingLootManager manager, LootContainersList lootList)
    {
        var containers = lootList.LootContainers;
        int count = containers.Length;

        float[] nextTimes = new float[count];
        float[] currentTimes = new float[count];

        for (int i = 0; i < count; i++) {
            if (containers[i] == null) continue;

            var id = containers[i].Definition.Id;

            nextTimes[i] = manager.NextSpawnTime.TryGetValue(id, out float nextTime) ? nextTime : 0f;
            currentTimes[i] = manager.CurrentSpawnTime.TryGetValue(id, out float currentTime) ? currentTime : 0f;
        }

        return new DriftingLootSystemData()
        {
            NextSpawnTime = nextTimes,
            CurrentSpawnTime = currentTimes,
            SwimmingDriftingLoot = SwimmingDriftingLootData.Create(manager.SpawnedSwimmingDriftingLoot),
            FlyingDriftingLoot = FlyingDriftingLootData.Create(manager.SpawnedFlyingDriftingLoot),
        };
    }
}