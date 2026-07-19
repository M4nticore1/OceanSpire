using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SwimmingDriftingLootData : DriftingLootData
{
    public bool Focused = false;

    public static SwimmingDriftingLootData Default()
    {
        return new SwimmingDriftingLootData();
    }

    public static SwimmingDriftingLootData Create(SwimmingDriftingLoot driftingLoot)
    {
        var data = new SwimmingDriftingLootData();
        data.Fill(driftingLoot);

        data.Focused = driftingLoot.FocusComponent.IsFocused;

        return data;
    }

    public static SwimmingDriftingLootData[] Create(IReadOnlyList<SwimmingDriftingLoot> driftingLoot)
    {
        List<SwimmingDriftingLootData> lootData = new();

        foreach (var loot in driftingLoot) {
            if (!loot) continue;

            lootData.Add(Create(loot));
        }

        return lootData.ToArray();
    }
}