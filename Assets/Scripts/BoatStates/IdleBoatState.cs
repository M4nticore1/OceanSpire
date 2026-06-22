using UnityEngine;

public class IdleBoatState : BoatState
{
    private const float correctDockPositionSpeed = 0.1f;
    private const float correctDockRotationSpeed = 0.5f;

    public IdleBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        boat.Movement.SetAgentEnabled(false);

        if (boat.CurrentRider) {
            boat.CurrentRider.HandleBoatSetedIdle(boat);
        }
    }

    public override void Exit()
    {
        boat.Movement.SetAgentEnabled(true);
    }

    public override void Tick()
    {
        if (!boat.DockPoint) return;
        if (boat.transform.rotation == boat.DockPoint.DockTransform.rotation) return;

        boat.transform.position = Vector3.Lerp(boat.transform.position, boat.DockPoint.DockTransform.position, correctDockPositionSpeed * Time.deltaTime);
        boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, boat.DockPoint.DockTransform.rotation, correctDockRotationSpeed * Time.deltaTime);
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        boat.SetState(BoatStateEnum.MovingToDock);
    }
}