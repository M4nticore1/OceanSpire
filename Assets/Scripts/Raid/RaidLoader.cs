using UnityEngine;

public class RaidLoader : Loader
{
    [SerializeField] private RaidManager raidManager;

    protected override void Load(WorldData data)
    {
        if (data != null && data.Raid != null) {
            LoadRaid(data.Raid);
        }
        else {
            InitRaid();
        }
    }

    private void LoadRaid(RaidData raidData)
    {
        raidManager.Init(raidData);
    }

    private void InitRaid()
    {
        var raidData = new RaidData() {
            RaidExist = false,
            UnderRaid = false,
            RaidCooldown = (int)raidManager.CalculateRandomCooldown(),
            TimeSinceLastRaid = 0,
        };

        raidManager.Init(raidData);
    }
}