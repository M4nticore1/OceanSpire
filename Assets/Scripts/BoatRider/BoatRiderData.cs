using System;
using UnityEngine;

[Serializable]
public class BoatRiderData
{
    public int? TargetBoatInstanceId = null;
    public int? RidingBoatInstanceId = null;

    public static BoatRiderData Create(BoatRider boatRider)
    {
        return new BoatRiderData()
        {
            TargetBoatInstanceId = boatRider.TargetBoat?.InstanceId.GetInstanceId(),
            RidingBoatInstanceId = boatRider.RidingBoat?.InstanceId.GetInstanceId(),
        };
    }
}