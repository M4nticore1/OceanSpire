using UnityEngine;

public class BoatIdleState : BoatState
{
    private const float correctDockRotationSpeed = 0.5f;

    public BoatIdleState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        if (!boat.currentRider) return;

        boat.currentRider.OnBoatSetedIdle();
        boat.Movement.SetAgentEnabled(false);

        Human human = boat.currentRider.GetComponent<Human>();
        if (human.currentStatusEnum == HumanStatusEnum.Wanderer) {
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

        boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, boat.dockPoint.DockTransform.rotation, correctDockRotationSpeed * Time.deltaTime);
    }

    public override void OnReachedPath()
    {

    }
}