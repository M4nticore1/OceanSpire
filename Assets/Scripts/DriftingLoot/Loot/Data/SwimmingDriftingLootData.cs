using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwimmingDriftingLootData : DriftingLootData
{
    public ItemData[] Items;

    public static SwimmingDriftingLootData Create(SwimmingDriftingLoot driftingLoot)
    {
        var data = new SwimmingDriftingLootData()
        {
            Items = ItemData.Create(driftingLoot.GetContainedLoot())
        };

        data.Fill(driftingLoot);

        return data;
    }

    public static SwimmingDriftingLootData[] Create(SwimmingDriftingLoot[] driftingLoot)
    {
        List<SwimmingDriftingLootData> lootData = new();

        foreach (var loot in driftingLoot) {
            lootData.Add(Create(loot));
        }

        return lootData.ToArray();
    }
}