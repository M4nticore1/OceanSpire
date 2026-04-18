using UnityEngine;

public class BoatRiderData
{
    public int boatInstanceId { get; private set; } = 0;
    public bool isRiding { get; private set; } = false;

    public BoatRiderData(int boatInstanceId, bool isRiding)
    {
        this.boatInstanceId = boatInstanceId;
        this.isRiding = isRiding;
    }

    public void SetBoatInstanceId(int value)
    {
        boatInstanceId = value;
    }

    public void SetRiding(bool value)
    {
        isRiding = value;
    }
}