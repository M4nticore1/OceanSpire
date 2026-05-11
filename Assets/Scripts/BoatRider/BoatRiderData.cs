using System;
using UnityEngine;

[Serializable]
public class BoatRiderData
{
    public int? BoatInstanceId = null;
    public bool IsRiding = false;

    public void SetBoatInstanceId(int value)
    {
        BoatInstanceId = value;
    }

    public void SetRiding(bool value)
    {
        IsRiding = value;
    }

    public static BoatRiderData Create(BoatRider boatRider)
    {
        return new BoatRiderData()
        {
            BoatInstanceId = boatRider.SelectedBoat?.InstanceId.Id,
            IsRiding = boatRider.IsRidingOnBoat
        };
    }
}