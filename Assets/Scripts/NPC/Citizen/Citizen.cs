using System;
using UnityEngine;

public class Citizen : Human
{
    public bool IsEvicted { get; private set; } = false;
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
        LeavePosition = evictData.LeavePosition;
        BoatRider.TrySetTargetBoat(evictData.Boat);

        OnCitizenEvicted?.Invoke(this);
    }

    public override bool ShouldClick()
    {
        if (!base.ShouldClick()) return false;
        if (IsEvicted) return false;
        if (BoatRider.RidingBoat && BoatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;

        return true;
    }

    public bool IsCitizenAvaliable()
    {
        if (IsEvicted) return false;
        if (!HealthComponent.IsAlive) return false;

        return true;
    }

    protected override void OnInit(CreatureData creatureData)
    {
        var citizenData = creatureData as CitizenData;
        if (citizenData == null) {
            Debug.Log($"Citizen Data not found at {name}");
            return;
        }

        IsEvicted = citizenData.Evicted;
        LeavePosition = citizenData.LeavePosition.Vector3();

        base.OnInit(creatureData);
    }

    protected override void DetermineNextAction()
    {
        if (ShouldBoatFindLoot()) {
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            BoatFloatAway();
            return;
        }

        base.DetermineNextAction();
    }

    protected override void BoatFindLoot()
    {
        var boat = BoatRider.RidingBoat;
        boat.SetState(BoatStateEnum.FindingLoot);
    }

    protected override void BoatFloatAway()
    {
        var boat = BoatRider.RidingBoat;
        boat.FloatAway(LeavePosition);
    }

    protected override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (InteractComponent.InteractBuilding && InteractComponent.InteractBuilding.GetComponent<PierModule>()) return false;

        return true;
    }

    protected override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;

        if (BoatRider.RidingBoat != BoatRider.TargetBoat) return false;
        if (!IsEvicted) return false;

        return true;
    }

    protected override bool ShouldBoatFindLoot()
    {
        if (!base.ShouldBoatFindLoot()) return false;

        if (IsEvicted) return false;
        if (BoatRider.RidingBoat != BoatRider.TargetBoat) return false;
        if (!InteractComponent.InteractBuilding) return false;
        if (!InteractComponent.InteractBuilding.GetComponent<PierModule>()) return false;

        return true;
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
        building.WorkComponent.AddCurrentWorker(this);

        base.OnInteractionStarted(building);
    }

    protected override void OnInteractionStopped(Building building)
    {
        building.WorkComponent.RemoveCurrentWorker(this);

        base.OnInteractionStopped(building);
    }

    protected override void OnDied()
    {
        base.OnDied();

        InteractComponent.RemoveInteractBuilding();
    }
}