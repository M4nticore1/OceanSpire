using UnityEngine;

public class BoatDockData
{
    public int InstanceId = 0;

    public static BoatDockData Create(BoatDockPoint boatDock)
    {
        return new BoatDockData()
        {
            InstanceId = boatDock.InstanceId.Id,
        };
    }
}
