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
        boat.Movement.SetAgentEnabled(false);

        Human human = boat.rider.GetComponent<Human>();
        if (human.currentStateEnum == HumanStateEnum.Wanderer) {
            boat.SelectComponent.SetClickable(true);
            human.SelectComponent.SetClickable(true);
        }
    }

    public override void Exit()
    {
        boat.Movement.SetAgentEnabled(true);
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