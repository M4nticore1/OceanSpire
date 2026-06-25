using System;
using UnityEngine;

[Serializable]
public class BoatRiderData
{
    public Guid? TargetBoatInstanceId;
    public Guid? RidingBoatInstanceId;

    public static BoatRiderData Default()
    {
        return new BoatRiderData();
    }

    public static BoatRiderData Create(BoatRider boatRider)
    {
        return new BoatRiderData()
        {
            TargetBoatInstanceId = boatRider.TargetBoat?.InstanceId.GetGuid(),
            RidingBoatInstanceId = boatRider.RidingBoat?.InstanceId.GetGuid(),
        };
    }
}