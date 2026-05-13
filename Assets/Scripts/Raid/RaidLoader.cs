using UnityEngine;

public class RaidLoader : Loader
{
    [SerializeField] private RaidManager raidManager;

    protected override void Load(WorldData data)
    {
        if (data == null && data.Raid != null) {
            LoadRaid(data.Raid);
        }
    }

    private void LoadRaid(RaidData raidData)
    {
        raidManager.Init(raidData);
    }
}