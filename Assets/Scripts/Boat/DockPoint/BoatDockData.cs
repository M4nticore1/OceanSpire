using System;
using UnityEngine;

public class BoatDockData
{
    public Guid InstanceId;

    public static BoatDockData Create(BoatDockPoint boatDock)
    {
        return new BoatDockData()
        {
            InstanceId = boatDock.InstanceId.GetGuid(),
        };
    }

    public static BoatDockData[] Create(BoatDockPoint[] boatDocks)
    {
        var boatDocksData = new BoatDockData[boatDocks.Length];

        for (int i = 0; i < boatDocks.Length; i++) {
            boatDocksData[i] = Create(boatDocks[i]);
        }

        return boatDocksData;
    }
}