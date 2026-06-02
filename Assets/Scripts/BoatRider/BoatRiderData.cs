using System;
using UnityEngine;

[Serializable]
public class BoatRiderData
{
    public int? BoatInstanceId = null;
    public bool Riding = false;
    public bool MovingToBoat = false;

    public static BoatRiderData Create(BoatRider boatRider)
    {
        return new BoatRiderData()
        {
            BoatInstanceId = boatRider.SelectedBoat?.InstanceId.GetInstanceId(),
            Riding = boatRider.IsRidingOnBoat,
            MovingToBoat = boatRider.IsMovingToBoat
        };
    }
}