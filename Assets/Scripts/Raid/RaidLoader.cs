using UnityEngine;

public class RaidLoader : WorldLoader
{
    [SerializeField] private RaidManager raidManager;

    protected override void Load(WorldData worldData)
    {
        var raidData = worldData?.Raid;

        if (raidData != null) {
            raidManager.Init(raidData);
        }
        else {
            raidManager.Init();
        }
    }
}