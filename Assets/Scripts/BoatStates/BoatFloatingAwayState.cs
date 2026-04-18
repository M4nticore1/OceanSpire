using UnityEngine;

public class BoatFloatingAwayState : BoatState
{
    public BoatFloatingAwayState(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        boat.RemoveDockPoint();
    }

    public override void Exit()
    {

    }

    public override void Process()
    {

    }

    public override void OnReachedPath()
    {
        Object.Destroy(boat.gameObject);
    }
}