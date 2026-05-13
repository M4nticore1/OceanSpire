using System;
using UnityEngine;

[Serializable]
public class RaidData
{
    public bool RaidExist = false;
    public bool UnderRaid = false;
    public int RaidCooldown = 0;
    public int TimeSinceLastRaid = 0;

    public static RaidData Create(RaidManager raidManager)
    {
        return new RaidData()
        {
            RaidExist = raidManager.IsRaidExist,
            UnderRaid = raidManager.IsUnderRaid,
            RaidCooldown = (int)raidManager.CurrentRaidCooldown,
            TimeSinceLastRaid = (int)raidManager.CurrentRaidCooldownTime,
        };
    }
}