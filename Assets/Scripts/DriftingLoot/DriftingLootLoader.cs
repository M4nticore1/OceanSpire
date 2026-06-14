using UnityEngine;

public class DriftingLootLoader : WorldLoader
{
    [SerializeField] private DriftingLootManager driftingLootManager;

    protected override void Load(WorldData worldData)
    {
        driftingLootManager.Init(worldData != null ? worldData.DriftingLoot : null);
    }
}