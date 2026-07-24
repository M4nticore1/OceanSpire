using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WandererData : HumanData
{
    public bool Rejected = false;
    public Vector3Data SpawnPosition = Vector3Data.Zero();

    public static WandererData Create(Wanderer wanderer)
    {
        var wandererData = new WandererData();
        wandererData.FillHumanData(wanderer);

        wandererData.Rejected = wanderer.IsRejected;
        wandererData.SpawnPosition = new Vector3Data(wanderer.SpawnPosition);

        return wandererData;
    }

    public static List<WandererData> Create(IReadOnlyList<Wanderer> wanderers)
    {
        var wanderersData = new List<WandererData>();

        foreach (var wanderer in wanderers) {
            if (!wanderer) continue;

            var data = Create(wanderer);
            if (data == null) continue;

            wanderersData.Add(data);
        }

        return wanderersData;
    }
}