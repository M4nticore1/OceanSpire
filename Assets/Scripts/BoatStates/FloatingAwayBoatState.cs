using UnityEngine;

public class FloatingAwayBoatState : BoatState
{
    public FloatingAwayBoatState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        boat.RemoveTargetLoot();
        boat.Movement.SetAgentEnabled(true);
    }

    public override void Exit()
    {

    }

    public override void Tick()
    {

    }

    public override void OnReachedPath()
    {
        Object.Destroy(boat.gameObject);
    }

    public override void OnBoatDockChanged(BoatDockPoint boatDock)
    {

    }
}