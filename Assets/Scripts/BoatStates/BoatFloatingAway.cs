using UnityEngine;

public class BoatFloatingAway : BoatState
{
    public BoatFloatingAway(Boat boat) : base(boat)
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

    public override void HandleReachedPath()
    {
        Object.Destroy(boat.gameObject);
    }
}