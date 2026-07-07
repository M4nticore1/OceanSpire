using System;
using UnityEngine;

[Serializable]
public class RaidData
{
    public bool RaidExist = false;
    public bool UnderRaid = false;
    public int RaidCooldownTime = 0;
    public int TimeSinceLastRaid = 0;

    public static RaidData Default()
    {
        return new RaidData();
    }

    public static RaidData Create(RaidManager raidManager)
    {
        return new RaidData()
        {
            RaidExist = raidManager.IsRaidExist,
            UnderRaid = raidManager.IsUnderRaid,
            RaidCooldownTime = (int)raidManager.RaidCooldownTime,
            TimeSinceLastRaid = (int)raidManager.TimeSinceLastRaid,
        };
    }
}