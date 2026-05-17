using UnityEngine;

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

    public static RaiderData[] Create(Raider[] wanderers)
    {
        var wanderersData = new RaiderData[wanderers.Length];

        for (int i = 0; i < wanderersData.Length; i++) {
            wanderersData[i] = Create(wanderers[i]);
        }

        return wanderersData;
    }
}