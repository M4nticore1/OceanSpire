using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlyingDriftingLootData : DriftingLootData
{
    public bool IsFalling = false;

    public static FlyingDriftingLootData Create(FlyingDriftingLoot driftingLoot)
    {
        var data = new FlyingDriftingLootData()
        {
            IsFalling = driftingLoot.IsFalling,
        };

        data.Fill(driftingLoot);

        return data;
    }

    public static FlyingDriftingLootData[] Create(FlyingDriftingLoot[] driftingLoot)
    {
        List<FlyingDriftingLootData> lootData = new();

        foreach (var loot in driftingLoot) {
            lootData.Add(Create(loot));
        }

        return lootData.ToArray();
    }
}