using UnityEngine;

public class Citizen : Human
{
    public bool IsEvicted { get; private set; } = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        CreaturesManager.Instance.RegisterCitizen(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.Instance.UnregisterCitizen(this);
    }

    public void Evict()
    {
        IsEvicted = true;
    }

    protected override void OnInit(CreatureData creatureData)
    {
        base.OnInit(creatureData);
    }

    protected override void OnSetedInteractBuilding(Building building)
    {
        base.OnSetedInteractBuilding(building);

        InteractComponent.AssignWorkerIndex();
        building.WorkComponent.AddWorker(InteractComponent);

        if (BoatRider.IsRidingOnBoat) {
            BoatRider.SelectedBoat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    protected override void OnRemovedInteractBuilding(Building building)
    {
        base.OnRemovedInteractBuilding(building);

        building.WorkComponent.RemoveWorker(InteractComponent);
    }

    protected override void OnInteractionStarted(Building building)
    {
        base.OnInteractionStarted(building);

        building.WorkComponent.EnterWorker(InteractComponent);
    }

    protected override void OnInteractionStopped(Building building)
    {
        base.OnInteractionStopped(building);

        building.WorkComponent.ExitWorker(InteractComponent);
    }

    protected override void OnEnteredBoat(Boat boat)
    {
        base.OnEnteredBoat(boat);

        boat.SetState(BoatStateEnum.FindingLoot);
    }

    protected override void OnBoatSetedIdle()
    {
        base.OnBoatSetedIdle();

        BoatRider.StartExitingBoat();
    }

    protected override void OnAttackStarted()
    {
        base.OnAttackStarted();

        InteractComponent.StopInteracting();
    }

    protected override void OnAttackStopped()
    {
        base.OnAttackStopped();

        var interactBuilding = InteractComponent.InteractBuilding;
        if (!interactBuilding) return;

        CityNavigator.TryFindPathToTargetBuilding();
    }

    protected override void OnDied()
    {
        base.OnDied();

        InteractComponent.RemoveInteractBuilding();
    }
}