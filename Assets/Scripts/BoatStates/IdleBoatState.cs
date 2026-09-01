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
        boat.RemoveTargetLoot();
        boat.Movement.TryStopMoving();

        if (boat.CurrentRider) {
            boat.CurrentRider.HandleBoatSetIdle(boat);
        }
    }

    public override void Exit()
    {

    }

    public override void Tick()
    {
        if (boat.DockPoint == null) return;
        if (boat.transform.rotation == boat.DockPoint.DockTransform.rotation) return;

        boat.transform.position = Vector3.Lerp(boat.transform.position, boat.DockPoint.DockTransform.position, correctDockPositionSpeed * Time.deltaTime);
        boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, boat.DockPoint.DockTransform.rotation, correctDockRotationSpeed * Time.deltaTime);
    }

    public override void OnReachedPath()
    {

    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {
        if (boatDock != null) {
            boat.SetState(BoatStateEnum.MovingToDock);
        }
    }
}