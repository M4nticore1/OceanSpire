using UnityEngine;

public class RaidLoader : WorldLoader
{
    [SerializeField] private RaidManager raidManager;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Raid != null) {
            raidManager.Init(data.Raid);
        }
        else {
            raidManager.Init();
        }
    }
}