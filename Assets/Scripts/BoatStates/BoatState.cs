using UnityEngine;

public abstract class BoatState
{
    protected Boat boat = null;

    public BoatState(Boat boat)
    {
        this.boat = boat;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Process();
    public abstract void HandleReachedPath();
}