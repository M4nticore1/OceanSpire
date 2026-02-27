using UnityEngine;

public class BoatIdleState : BoatState
{
    public BoatIdleState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (!boat.rider) return;

        boat.rider.HandleBoatSetedIdle();
    }

    public override void Exit()
    {

    }

    public override void Process()
    {
        if (boat.transform.rotation == boat.dockPoint.DockTransform.rotation) return;

        boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, boat.dockPoint.DockTransform.rotation, BoatData.correctDockRotationSpeed * Time.deltaTime);
    }

    public override void HandleReachedPath()
    {

    }
}