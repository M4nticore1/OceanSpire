using UnityEngine;

public class BoatIdleState : BoatState
{
    private const float correctDockRotationSpeed = 0.5f;

    public BoatIdleState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (!boat.SelectedRider) return;

        boat.SelectedRider.OnBoatSetedIdle();
        boat.Movement.SetAgentEnabled(false);
    }

    public override void Exit()
    {
        boat.Movement.SetAgentEnabled(true);
    }

    public override void Tick()
    {
        if (boat.transform.rotation == boat.dockPoint.DockTransform.rotation) return;

        boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, boat.dockPoint.DockTransform.rotation, correctDockRotationSpeed * Time.deltaTime);
    }

    public override void OnReachedPath()
    {

    }
}