using System;
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

    public static WandererData[] Create(Wanderer[] wanderers)
    {
        var wanderersData = new WandererData[wanderers.Length];

        for (int i = 0; i < wanderersData.Length; i++) {
            wanderersData[i] = Create(wanderers[i]);
        }

        return wanderersData;
    }
}