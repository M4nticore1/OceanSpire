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

    public void Evict(Boat boat, Vector3 leavePosition)
    {
        IsEvicted = true;
        LeavePosition = leavePosition;
        BoatRider.TrySetTargetBoat(boat);

        AttackComponent.RemoveTarget();
        AttackComponent.RemoveAllAttackers();

        HealthComponent.SetCurrentHealth(HealthComponent.MaxHealth);
        HealthDisplay.Hide();

        SelectComponent.Deselect();

        OnCitizenEvicted?.Invoke(this);
    }

    public override bool ShouldClick()
    {
        if (!base.ShouldClick()) return false;
        if (IsEvicted) return false;
        //if (BoatRider.RidingBoat && BoatRider.RidingBoat.CurrentStateEnum != BoatStateEnum.Idle) return false;

        return true;
    }

    protected override void OnClick()
    {
        base.OnClick();

        SelectComponent.Click();
    }

    protected override void HandleInit(CreatureData creatureData)
    {
        var citizenData = creatureData as CitizenData;
        if (citizenData == null) {
            Debug.Log($"Citizen Data is not valid", this);
            return;
        }

        if (citizenData.EvictData != null) {
            IsEvicted = citizenData.EvictData.Evicted;
            LeavePosition = citizenData.EvictData.LeavePosition.Vector3();
        }
        else {
            Debug.LogError("Evict Data is not valid", this);
        }

        base.HandleInit(creatureData);
    }

    protected override CreatureData GetDefaultData()
    {
        return CitizenData.Default();
    }

    protected override void DetermineNextAction()
    {
        if (ShouldBoatMoveToDock()) {
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFindLoot()) {
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            BoatFloatAway();
            return;
        }
        if (ShouldStartAttacking()) {
            StartAttacking();
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
        boat.RemoveDockPoint();
    }

    protected override void StartAttacking()
    {
        var currentBuilding = CityNavigator.CurrentBuilding;
        var currentRaiders = currentBuilding.RaidersHandler.CurrentInteractors;

        foreach (var interactor in currentRaiders) {
            if (!interactor.HealthComponent.IsAlive) continue;

            var raider = interactor as Raider;
            if (!raider) continue;
            if (!raider.IsRaidingBuilding) continue;

            AttackComponent.SetTarget(interactor.AttackComponent);
            AttackComponent.MoveToTarget();
            break;
        }
    }

    public override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (InteractComponent.InteractBuilding && InteractComponent.InteractBuilding.GetComponent<PierModule>()) return false;

        return true;
    }

    public override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;

        if (!IsEvicted) return false;
        if (!HealthComponent.IsAlive) return false;
        if (BoatRider.RidingBoat != BoatRider.TargetBoat) return false;

        return true;
    }

    //public override bool ShouldBoatFindLoot()
    //{
    //    if (!base.ShouldBoatFindLoot()) return false;

    //    //var ridingBoat = BoatRider.RidingBoat;
    //    //if (ridingBoat != BoatRider.TargetBoat) return false;
    //    //if (!ridingBoat.ShouldFindLoot()) return false;

    //    //return true;
    //}

    public override bool ShouldStartAttacking()
    {
        if (!base.ShouldStartAttacking()) return false;

        var currentBuilding = CityNavigator.CurrentBuilding;
        if (!currentBuilding) return false;

        if (currentBuilding != InteractComponent.InteractBuilding) return false;

        var currentRaiders = currentBuilding.RaidersHandler.CurrentInteractors;
        foreach (var raider in currentRaiders) {
            if (!raider) continue;
            if (!raider.HealthComponent.IsAlive) continue;

            return true;
        }

        return false;
    }

    protected override void OnInteractBuildingSeted(Building building)
    {
        building.CitizensHandler.AddInteractor(this);

        base.OnInteractBuildingSeted(building);
    }

    protected override void OnInteractBuildingRemoved(Building building)
    {
        building.CitizensHandler.RemoveInteractor(this);

        base.OnInteractBuildingRemoved(building);
    }

    protected override void OnInteractionStarted(Building building)
    {
        building.CitizensHandler.AddCurrentInteractor(this);

        base.OnInteractionStarted(building);
    }

    protected override void OnInteractionStopped(Building building)
    {
        building.CitizensHandler.RemoveCurrentInteractor(this);

        base.OnInteractionStopped(building);
    }

    protected override void OnDied()
    {
        base.OnDied();

        var interactBuilding = InteractComponent.InteractBuilding;
        InteractComponent.RemoveInteractBuilding();
        InteractComponent.TryStopInteracting(interactBuilding);
    }

    public bool IsCitizenAvaliable()
    {
        if (IsEvicted) return false;
        if (!HealthComponent.IsAlive) return false;

        return true;
    }
}