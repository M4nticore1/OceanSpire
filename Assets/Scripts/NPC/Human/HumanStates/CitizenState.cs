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
        human.Interactor.AssignWorkerIndex();
        building.AddWorker(human.Interactor);

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
        human.Interactor.AssignWorkerIndex();
        human.Interactor.interactBuilding.RemoveWorker(human.Interactor);
        human.Interactor.RemoveInteractBuilding();

        EventBus.InvokeRemovedWorkBuilding();
    }

    public override void OnStoppedMoving()
    {
        if (!human.Interactor.interactBuilding) return;
        if (human.Interactor.interactBuilding != human.CityNavigator.currentBuilding) return;

        human.Interactor.StartInteracting();
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
        BuildingInteractHandler interactor = human.Interactor;

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