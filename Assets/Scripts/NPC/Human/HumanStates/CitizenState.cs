using UnityEngine;

public class CitizenState : HumanState
{
    public CitizenState (Human human) : base(human) {

    }

    public override void Enter()
    {
        CreaturesManager.instance.RegisterCitizen(human);

        human.Movement.SetMovementMethod(MovementMethod.Walk);
    }

    public override void Exit()
    {
        CreaturesManager.instance.UnregisterCitizen(human);
    }

    public override void Tick()
    {

    }

    public override void OnStartedAttacking()
    {

    }

    public override void OnStoppedAttacking()
    {

    }

    public override void OnSetedInteractBuilding(Building building)
    {
        human.InteractComponent.AssignWorkerIndex();
        building.AddWorker(human.InteractComponent);

        if (human.BoatRider.isRidingOnBoat) {
            human.BoatRider.selectedBoat.SetState(BoatStateEnum.FindingLoot);
        }
        else {
            human.CityNavigator.TryFindPathToTargetBuilding();
        }

        EventBus.InvokeSetedWorkBuilding();
    }

    public override void OnRemovedInteractBuilding()
    {
        human.InteractComponent.AssignWorkerIndex();
        human.InteractComponent.interactBuilding.RemoveWorker(human.InteractComponent);
        human.InteractComponent.RemoveInteractBuilding();

        EventBus.InvokeRemovedWorkBuilding();
    }

    public override void OnStoppedMoving()
    {
        if (!human.InteractComponent.interactBuilding) return;
        if (human.InteractComponent.interactBuilding != human.CityNavigator.currentBuilding) return;

        human.InteractComponent.StartInteracting();
    }

    public override void OnEnteredBuilding(Building building)
    {

    }

    public override void OnEnteredBoat(Boat boat)
    {
        human.BoatRider.selectedBoat.SetState(BoatStateEnum.FindingLoot);
    }

    public override void OnExitedBoat(Boat boat)
    {
        
    }

    public override void OnRevived()
    {
        EventBus.InvokeCitizenRevived(human);
    }

    public override void OnDied()
    {
        InteractComponent interactor = human.InteractComponent;

        if (interactor) {
            interactor.RemoveInteractBuilding();
        }

        EventBus.InvokeCitizenDied(human);
    }

    public override void OnDisable()
    {
        CreaturesManager.instance.UnregisterCitizen(human);
    }
}