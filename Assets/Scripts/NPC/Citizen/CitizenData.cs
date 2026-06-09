using System;
using UnityEngine;

[Serializable]
public class CitizenData : HumanData
{
    public bool Evicted = false;
    public Vector3Data LeavePosition = Vector3Data.Zero();

    public static CitizenData Create(Citizen citizen)
    {
        var citizenData = new CitizenData();
        citizenData.FillHumanData(citizen);

        citizenData.Evicted = citizen.IsEvicted;
        citizenData.LeavePosition = new Vector3Data(citizen.LeavePosition);

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