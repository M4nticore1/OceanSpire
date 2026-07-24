using System;
using System.Collections.Generic;
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

    public static List<CitizenData> Create(IReadOnlyList<Citizen> citizens)
    {
        var citizenData = new List<CitizenData>();

        foreach (var citizen in citizens) {
            if (!citizen) continue;

            var data = Create(citizen);
            if (data == null) continue;

            citizenData.Add(Create(citizen));
        }

        return citizenData;
    }
}