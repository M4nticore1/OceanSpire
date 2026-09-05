using System;
using UnityEngine;

public class Citizen : Human
{
    [Header("Citizen")]
    [field: SerializeField] public bool IsEvicted { get; private set; } = false;
    [field: SerializeField] public Vector3 LeavePosition { get; private set; }

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

        InteractComponent.RemoveInteractBuilding();
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
            Debug.Log($"[{nameof(Citizen)}] Citizen Data is not valid");
            return;
        }

        if (citizenData.EvictData != null) {
            IsEvicted = citizenData.EvictData.Evicted;
            LeavePosition = citizenData.EvictData.LeavePosition.Vector3();
        }
        else {
            Debug.LogError($"{nameof(Citizen)}] Evict Data is not valid");
        }

        base.HandleInit(creatureData);

        if (IsEvicted) {
            InteractComponent.RemoveInteractBuilding();
        }
    }

    protected override CreatureData GetDefaultData()
    {
        return CitizenData.Default();
    }

    // Actions
    protected override void DetermineNextAction()
    {
        if (ShouldBoatFindLoot()) {
            //Debug.Log("BoatFindLoot");
            BoatFindLoot();
            return;
        }
        if (ShouldBoatFloatAway()) {
            //Debug.Log("BoatFloatAway");
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
        boat.RemoveDockPoint();
    }

    public override bool ShouldBoatFindLoot()
    {
        if (!base.ShouldBoatFindLoot()) return false;
        if (IsEvicted) return false;

        return true;
    }

    public override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (IsEvicted) return true;
        //if (InteractComponent.InteractBuilding && InteractComponent.InteractBuilding.GetComponent<PierModule>()) return false;

        return true;
    }

    public override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;

        if (!IsEvicted) return false;
        if (!HealthComponent.IsAlive) return false;

        var ridingBoat = BoatRider.RidingBoat;
        var targetBoat = BoatRider.TargetBoat;
        if (ridingBoat != targetBoat) return false;

        return true;
    }

    // Fix Boat
    protected override void UpdateTargetBoat()
    {
        if (IsEvicted) {
            BoatRider.TrySetTargetBoat(boatsManager.GetFirstFreeBoat(boatsManager.EvictBoats));
        }
        else {
            BoatRider.TrySetTargetBoat(boatsManager.GetFirstFreeBoat(boatsManager.CitizenBoats));
        }
    }

    protected override void UpdateRidingBoat()
    {
        
    }

    protected override bool ShouldUpdateTargetBoat()
    {
        if (!base.ShouldUpdateTargetBoat()) return false;

        if (!IsEvicted) {
            var interactBuilding = InteractComponent.InteractBuilding;
            if (interactBuilding == null) return false;

            var pierModule = interactBuilding.GetComponent<PierModule>();
            if (pierModule == null) return false;
        }

        return true;
    }

    protected override bool ShouldUpdateRidingBoat()
    {
        return false;
    }

    // Interact Building
    protected override void HandleInteractBuildingSet(Building building)
    {
        building.CitizensHandler.AddInteractor(this);

        base.HandleInteractBuildingSet(building);
    }

    protected override void HandleInteractBuildingRemoved(Building building)
    {
        building.CitizensHandler.RemoveInteractor(this);

        base.HandleInteractBuildingRemoved(building);
    }

    // Interaction
    protected override void HandleInteractionStarted(Building building)
    {
        building.CitizensHandler.AddCurrentInteractor(this);

        base.HandleInteractionStarted(building);
    }

    protected override void HandleInteractionStopped(Building building)
    {
        building.CitizensHandler.RemoveCurrentInteractor(this);

        base.HandleInteractionStopped(building);
    }

    // Dead
    protected override void HandleDied()
    {
        base.HandleDied();

        var interactBuilding = InteractComponent.InteractBuilding;
        InteractComponent.RemoveInteractBuilding();
        InteractComponent.TryStopInteracting(interactBuilding);
    }

    public bool IsCitizenAvailable()
    {
        if (IsEvicted) return false;
        if (!HealthComponent.IsAlive) return false;

        return true;
    }
}