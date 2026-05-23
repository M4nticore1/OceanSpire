using UnityEngine;

public class UpgradeData
{
    public int NextLevel;

    public UpgradeData Create(UpgradeComponent upgradeComponent)
    {
        return new UpgradeData() {
            NextLevel = upgradeComponent.NextLevel,
        };
    }
}