using System;
using UnityEngine;

[Serializable]
public class CitizenData : HumanData
{
    public bool Evicted = false;
    public int? EvictionBoatInstanceId = null;
    public Vector3Data? LeavePosition = null;

    public static CitizenData Create(Citizen citizen)
    {
        var citizenData = new CitizenData();
        citizenData.FillHumanData(citizen);

        citizenData.Evicted = citizen.IsEvicted;
        citizenData.EvictionBoatInstanceId = citizen.EvictionBoat?.InstanceId.GetId();
        citizenData.LeavePosition = citizenData.EvictionBoatInstanceId != null ? new Vector3Data(citizen.LeavePosition) : null;

        return citizenData;
    }

    public static CitizenData[] Create(Citizen[] citizens)
    {
        var citizenData = new CitizenData[citizens.Length];

        for (int i = 0; i < citizenData.Length; i++) {
            citizenData[i] = Create(citizens[i]);
        }

        return citizenData;
    }
}