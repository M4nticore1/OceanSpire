using UnityEngine;

public class CitizenState : HumanState
{
    public CitizenState (Human human) : base(human) {

    }

    public override void OnSetedInteractBuilding(Building building)
    {
        human.Interactor.AssignWorkerIndex();
        building.AddWorker(human.Interactor);

        EventBus.InvokeSetedWorkBuilding();
    }

    public override void OnRemovedInteractBuilding()
    {
        human.Interactor.AssignWorkerIndex();
        human.Interactor.InteractBuilding.RemoveWorker(human.Interactor);
        human.Interactor.RemoveInteractBuilding();

        EventBus.InvokeRemovedWorkBuilding();
    }

    public override void OnStoppedMoving()
    {
        if (!human.Interactor.InteractBuilding) return;
        if (human.Interactor.InteractBuilding != human.CityNavigator.currentBuilding) return;

        human.Interactor.StartInteracting();
    }
}
