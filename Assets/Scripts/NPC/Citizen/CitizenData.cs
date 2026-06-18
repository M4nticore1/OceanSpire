using System;
using UnityEngine;

[Serializable]
public class CitizenData : HumanData
{
    public EvictData EvictData = EvictData.Default();

    public static CitizenData Create(Citizen citizen)
    {
        var citizenData = new CitizenData();
        citizenData.FillHumanData(citizen);

        citizenData.EvictData = EvictData.Create(citizen);

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