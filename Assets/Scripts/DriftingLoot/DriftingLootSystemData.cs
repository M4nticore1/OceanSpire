using System;
using UnityEngine;

[Serializable]
public class DriftingLootSystemData
{
    public float[] NextSpawnTime;
    public float[] CurrentSpawnTime;
    public SwimmingDriftingLootData[] SwimmingDriftingLoot;
    public FlyingDriftingLootData[] FlyingDriftingLoot;

    public static DriftingLootSystemData Create(DriftingLootManager driftingLootManager)
    {
        return new DriftingLootSystemData()
        {
            NextSpawnTime = driftingLootManager.NextSpawnTime,
            CurrentSpawnTime = driftingLootManager.CurrentSpawnTime,
            SwimmingDriftingLoot = SwimmingDriftingLootData.Create(driftingLootManager.SpawnedSwimmingDriftingLoot.ToArray()),
            FlyingDriftingLoot = FlyingDriftingLootData.Create(driftingLootManager.SpawnedFlyingDriftingLoot.ToArray()),
        };
    }
}