using UnityEngine;

public class GoingToRidingElevatorPassengerState : ElevatorPassengerState
{
    public GoingToRidingElevatorPassengerState(ElevatorPassenger elevatorPassenger) : base(elevatorPassenger)
    {

    }

    public override void Enter()
    {
        if (EnteredElevator == null) return;
        if (CityNavigator == null) return;

        EnteredElevator.AddGoingToRidingPassenger(ElevatorPassenger);
        Debug.Log(EnteredElevator.SpawnedElevatorCabin.InteractionPointsHandler.InteractorsDict.Count);
        CityNavigator.Movement.TryMoveTo(EnteredElevator.GetCabinRidingTransform(ElevatorPassenger));
    }

    public override void Exit()
    {
        if (EnteredElevator == null) return;

        EnteredElevator.RemoveGoingToRidingPassenger(ElevatorPassenger);
    }
}