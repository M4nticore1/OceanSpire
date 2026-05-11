using System;
using UnityEngine;

public class CitizenState : HumanState
{
    public CitizenState (Human human) : base(human) {

    }

    public override void Enter()
    {
        CreaturesManager.Instance.RegisterCitizen(human);

        human.Movement.SetMovementMethod(MovementMethod.Walk);
    }

    public override void Exit()
    {
        CreaturesManager.Instance.UnregisterCitizen(human);
    }

    public override void Tick()
    {

    }

    public override void OnAttackStarted()
    {
        human.InteractComponent.StopInteracting();
    }

    public override void OnAttackStopped()
    {
        Building interactBuilding = human.InteractComponent.InteractBuilding;
        if (!interactBuilding) return;

        human.CityNavigator.TryFindPathToTargetBuilding();
    }

    public override void OnSetedInteractBuilding(Building building)
    {
        human.InteractComponent.AssignWorkerIndex();
        building.WorkComponent.AddWorker(human.InteractComponent);

        if (human.BoatRider.IsRidingOnBoat) {
            human.BoatRider.SelectedBoat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            human.CityNavigator.TryFindPathToTargetBuilding();
        }
    }

    public override void OnRemovedInteractBuilding(Building building)
    {
        building.WorkComponent.RemoveWorker(human.InteractComponent);
    }

    public override void OnInteractionStarted()
    {
        human.InteractComponent.InteractBuilding.WorkComponent.EnterWorker(human.InteractComponent);
    }

    public override void OnInteractionStopped()
    {
        human.InteractComponent.InteractBuilding.WorkComponent.ExitWorker(human.InteractComponent);
    }

    public override void OnStoppedMoving()
    {

    }

    public override void OnEnteredBuilding(Building building)
    {

    }

    public override void OnEnteredBoat(Boat boat)
    {
        human.BoatRider.SelectedBoat.SetState(BoatStateEnum.FindingLoot);
    }

    public override void OnExitedBoat(Boat boat)
    {
        
    }

    public override void OnRevived()
    {

    }

    public override void OnDied()
    {
        human.InteractComponent.RemoveInteractBuilding();
    }
}