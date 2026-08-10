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
        if (!boat) return;
       
        if (boat.ShouldFindLoot()) {
            boat.SetState(BoatStateEnum.FindingLoot);
        }
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (!boat) return false;

        var state = boat.CurrentStateEnum;
        if (state == BoatStateEnum.MovingToDock) return true;
        if (state == BoatStateEnum.UnloadingLoot) return true;

        return false;
    }

    protected override bool ShouldEnableButton()
    {
        if (!boat) return false;
        if (!boat.ShouldFindLoot()) return false;

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