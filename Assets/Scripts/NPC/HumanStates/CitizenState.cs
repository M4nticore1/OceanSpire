using UnityEngine;

public class CitizenState : HumanState
{
    public CitizenState (Human human) : base(human) {

    }

    public override void Tick()
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
}
