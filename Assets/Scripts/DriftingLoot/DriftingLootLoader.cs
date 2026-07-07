using UnityEngine;

public class DriftingLootLoader : WorldLoader
{
    [SerializeField] private DriftingLootManager driftingLootManager;

    protected override void Load(WorldData worldData)
    {
        var data = worldData?.DriftingLoot;

        if (data != null) {
            driftingLootManager.Init(data);
        }
        else {
            driftingLootManager.Init();
        }
    }
}