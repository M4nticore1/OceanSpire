using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RaiderData : HumanData
{
    public bool RaidFinished = false;
    public Vector3Data SpawnPosition = Vector3Data.Zero();

    public static RaiderData Create(Raider raider)
    {
        var raiderData = new RaiderData();
        raiderData.FillHumanData(raider);

        raiderData.RaidFinished = raider.IsRaidFinished;
        raiderData.SpawnPosition = new Vector3Data(raider.SpawnPosition);

        return raiderData;
    }

    public static List<RaiderData> Create(IReadOnlyList<Raider> raiders)
    {
        var raidersData = new List<RaiderData>();

        foreach (var raider in raiders) {
            if (!raider) {
                Debug.LogError("Raider is not valid");
                continue;
            }

            var data = RaiderData.Create(raider);
            if (data == null) {
                Debug.LogError("Raider data is not valid");
                continue;
            }

            raidersData.Add(data);
        }

        return raidersData;
    }
}