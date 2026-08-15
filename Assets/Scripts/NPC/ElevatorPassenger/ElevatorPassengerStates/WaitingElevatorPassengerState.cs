using UnityEngine;

public class WaitingElevatorPassengerState : ElevatorPassengerState
{
    public WaitingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        if (EnteredElevator == null) return;

        EnteredElevator.AddWaitingPassenger(ElevatorPassenger);
    }

    public override void Exit()
    {
        if (EnteredElevator == null) return;

        EnteredElevator.RemoveWaitingPassenger(ElevatorPassenger);
    }
}