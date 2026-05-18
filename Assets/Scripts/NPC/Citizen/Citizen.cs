using UnityEngine;

public class Citizen : Human
{
    public bool IsEvicted { get; private set; } = false;
    public Boat EvictionBoat { get; private set; }
    public Vector3 LeavePosition { get; private set; }

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

    public void Evict(Boat boat)
    {
        IsEvicted = true;
        EvictionBoat = boat;

        if (BoatRider.IsRidingOnBoat) {
            BoatRider.SelectedBoat.SetState(BoatStateEnum.MovingToDock);
        }
    }

    protected override void OnInit(CreatureData creatureData)
    {
        base.OnInit(creatureData);

        var citizenData = creatureData as CitizenData;

        IsEvicted = citizenData.Evicted;
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

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        boat.SetState(BoatStateEnum.FindingLoot);
    }

    protected override void HandleExitedBoat(Boat boat)
    {
        base.HandleExitedBoat(boat);

        if (IsEvicted) {
            BoatRider.SetSelectedBoat(EvictionBoat);
            BoatRider.TryMoveToBoat();
        }
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