using UnityEngine;

public abstract class ElevatorPassengerState
{
    protected ElevatorPassenger elevatorPassenger;

    public ElevatorPassengerState(ElevatorPassenger elevatorPassenger)
    {
        this.elevatorPassenger = elevatorPassenger;
    }

    public abstract void Enter();
    public abstract void Exit();
}