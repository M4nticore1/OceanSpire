using UnityEngine;

public class WaitingElevatorPassengerState : ElevatorPassengerState
{
    public WaitingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        var currentElevator = elevatorPassenger.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        currentElevator.AddWaitingPassenger(elevatorPassenger);
    }

    public override void Exit()
    {
        var currentElevator = elevatorPassenger.CurrentElevator;

        if (!currentElevator) {
            Debug.LogError("currentElevator is not valid");
            return;
        }

        currentElevator.RemoveWaitingPassenger(elevatorPassenger);
    }
}