using System;
using UnityEngine;

public class Citizen : Human
{
    public bool IsEvicted { get; private set; } = false;
    public Boat EvictionBoat { get; private set; }
    public Vector3 LeavePosition { get; private set; }

    public static event Action<Citizen> OnCitizenEvicted;

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

    public void Evict(EvictData evictData)
    {
        IsEvicted = true;
        EvictionBoat = evictData.Boat;
        LeavePosition = evictData.LeavePosition;

        InteractComponent.TryRemoveInteractBuilding();

        if (!BoatRider.RidingBoat) {
            BoatRider.TrySetTargetBoat(evictData.Boat);
        }

        BoatRider.MoveToBoat();

        OnCitizenEvicted?.Invoke(this);
    }

    public override bool ShouldClick()
    {
        if (!base.ShouldClick()) return false;
        if (IsEvicted) return false;

        return true;
    }

    protected override void OnInit(CreatureData creatureData)
    {
        var citizenData = creatureData as CitizenData;

        IsEvicted = citizenData.Evicted;

        base.OnInit(creatureData);
    }

    protected override void OnInteractBuildingSeted(Building building)
    {
        building.WorkComponent.AddWorker(this);

        base.OnInteractBuildingSeted(building);
    }

    protected override void OnInteractBuildingRemoved(Building building)
    {
        building.WorkComponent.RemoveWorker(this);

        base.OnInteractBuildingRemoved(building);
    }

    protected override void OnInteractionStarted(Building building)
    {
        base.OnInteractionStarted(building);

        building.WorkComponent.AddCurrentWorker(this);
    }

    protected override void OnInteractionStopped(Building building)
    {
        base.OnInteractionStopped(building);

        building.WorkComponent.ExitWorker(this);
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        if (IsEvicted) {
            boat.FloatAway(LeavePosition);
        }
        else {
            var building = InteractComponent.InteractBuilding;
            if (!building) {
                boat.SetState(BoatStateEnum.MovingToDock);
                return;
            }

            var pier = building.GetComponent<PierModule>();
            if (!pier) {
                boat.SetState(BoatStateEnum.MovingToDock);
                return;
            }

            boat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    protected override void HandleExitedBoat(Boat boat)
    {
        base.HandleExitedBoat(boat);

        if (IsEvicted) {
            BoatRider.TrySetTargetBoat(EvictionBoat);
            BoatRider.TryMoveToBoat();
        }
    }

    protected override void OnBoatSetedIdle(Boat boat)
    {
        base.OnBoatSetedIdle(boat);

        BoatRider.StartExitingBoat();
    }

    protected override void OnAttackStarted()
    {
        base.OnAttackStarted();

        InteractComponent.TryStopInteracting();
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