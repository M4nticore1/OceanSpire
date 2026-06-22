using UnityEngine;

public class BoatDockData
{
    public int InstanceId = 0;

    public static BoatDockData Create(BoatDockPoint boatDock)
    {
        return new BoatDockData()
        {
            InstanceId = boatDock.InstanceId.GetId(),
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
