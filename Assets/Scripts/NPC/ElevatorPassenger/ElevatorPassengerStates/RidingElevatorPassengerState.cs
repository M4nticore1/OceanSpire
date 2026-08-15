using UnityEngine;

public class RidingElevatorPassengerState : ElevatorPassengerState
{
    public RidingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        if (EnteredElevator == null) return;
        if (CityNavigator == null) return;

        EnteredElevator.AddRidingPassenger(ElevatorPassenger);
        CityNavigator.Movement.SetAgentEnabled(false);
        CityNavigator.transform.SetParent(EnteredElevator.SpawnedElevatorCabin.transform);
    }

    public override void Exit()
    {
        if (CityNavigator == null) return;

        var currentElevator = CityNavigator.CurrentElevator;
        if (currentElevator == null) return;

        CityNavigator.Movement.SetAgentEnabled(true);
        CityNavigator.transform.SetParent(null);
        currentElevator.RemoveRidingPassenger(ElevatorPassenger);
    }
}