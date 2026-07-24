using System;
using System.Collections.Generic;
using UnityEngine;

public class BoatDockData
{
    public Guid InstanceId = Guid.NewGuid();

    public static BoatDockData Default()
    {
        return new BoatDockData();
    }

    public static BoatDockData Create(BoatDockPoint boatDock)
    {
        return new BoatDockData()
        {
            InstanceId = boatDock.InstanceId.GetGuid(),
        };
    }

    public static List<BoatDockData> Create(IReadOnlyList<BoatDockPoint> boatDocks)
    {
        var boatDocksData = new List<BoatDockData>();

        foreach (var boatDock in boatDocks) {
            if (!boatDock) continue;

            var data = Create(boatDock);
            if (data == null) continue;

            boatDocksData.Add(data);
        }

        return boatDocksData;
    }
}