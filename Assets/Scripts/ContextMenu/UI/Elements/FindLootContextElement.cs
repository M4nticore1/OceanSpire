using UnityEngine;

public class FindLootContextElement : ContextElement
{
    private Boat boat;

    protected override void Subscribe()
    {
        base.Subscribe();

        Boat.OnBoatStateEntered += HandleBoatStateEntered;
        Boat.OnInventoryItemAmountChanged += HandleInventoryItemAmountChanged;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        Boat.OnBoatStateEntered -= HandleBoatStateEntered;
        Boat.OnInventoryItemAmountChanged -= HandleInventoryItemAmountChanged;
    }

    protected override void OnButtonClicked()
    {
        if (boat != null) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (boat == null) return false;

        var state = boat.CurrentStateEnum;
        if (state == BoatStateEnum.MovingToDock) return true;
        if (state == BoatStateEnum.UnloadingLoot) return true;

        return false;
    }

    protected override bool ShouldEnableButton()
    {
        if (boat == null) return false;

        if (boat.CurrentStateEnum == BoatStateEnum.CollectingLoot && boat.TargetDriftingLoot != null) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.UnloadingLoot && boat.Inventory.Items.Count > 0) return false;

        var currentRider = boat.CurrentRider;
        if (currentRider == null) return false;

        var targetBoat = currentRider.TargetBoat;
        if (targetBoat != null && targetBoat != boat) return false;

        var citizen = currentRider.GetComponent<Citizen>();
        if (citizen == null) return false;
        if (!citizen.IsCitizenAvailable()) return false;

        var interactBuilding = citizen.InteractComponent.InteractBuilding;
        if (interactBuilding == null) return false;

        var pier = interactBuilding.GetComponent<PierModule>();
        if (pier == null) return false;

        if (boat.IsOverweight()) return false;

        return true;
    }

    private void HandleBoatStateEntered(Boat boat)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
        UpdateButtonEnabled();
    }

    private void HandleInventoryItemAmountChanged(Boat boat, ItemInstance item)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
        UpdateButtonEnabled();
    }
}