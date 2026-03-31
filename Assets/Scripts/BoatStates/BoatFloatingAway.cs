using UnityEngine;

public class BoatFloatingAway : BoatState
{
    public BoatFloatingAway(Boat boat) : base(boat)
    {

    }

    public override void Enter()
    {
        Vector3 position = WorldUtils.GetRandomBorderPosition();
        boat.Movement.TryMoveTo(position);
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