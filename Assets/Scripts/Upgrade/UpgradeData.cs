using System;
using UnityEngine;

[Serializable]
public class UpgradeData
{
    public bool UnderUpgrade = false;
    public int NextLevel = 1;

    public static UpgradeData Default()
    {
        return new UpgradeData();
    }

    public static UpgradeData Create(UpgradeComponent upgradeComponent)
    {
        return new UpgradeData() {
            UnderUpgrade = upgradeComponent.IsUnderUpgrade,
            NextLevel = upgradeComponent.NextLevel,
        };
    }
}