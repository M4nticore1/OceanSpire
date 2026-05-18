using System;
using UnityEngine;

[Serializable]
public class CitizenData : HumanData
{
    public bool Evicted = false;

    public static CitizenData Create(Citizen citizen)
    {
        var wandererData = new CitizenData();
        wandererData.FillHumanData(citizen);

        wandererData.Evicted = citizen.IsEvicted;

        return wandererData;
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