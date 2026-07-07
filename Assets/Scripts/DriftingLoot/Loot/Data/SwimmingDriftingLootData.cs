using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwimmingDriftingLootData : DriftingLootData
{
    public static SwimmingDriftingLootData Default()
    {
        return new SwimmingDriftingLootData();
    }

    public static SwimmingDriftingLootData Create(SwimmingDriftingLoot driftingLoot)
    {
        var data = new SwimmingDriftingLootData();
        data.Fill(driftingLoot);

        return data;
    }

    public static SwimmingDriftingLootData[] Create(IReadOnlyList<SwimmingDriftingLoot> driftingLoot)
    {
        List<SwimmingDriftingLootData> lootData = new();

        foreach (var loot in driftingLoot) {
            if (loot == null) continue;

            lootData.Add(Create(loot));
        }

        return lootData.ToArray();
    }
}