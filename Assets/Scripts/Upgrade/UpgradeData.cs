using System;
using UnityEngine;

[Serializable]
public class UpgradeData
{
    public int NextLevel = 1;

    public static UpgradeData Create(UpgradeComponent upgradeComponent)
    {
        return new UpgradeData() {
            NextLevel = upgradeComponent.NextLevel,
        };
    }
}