using UnityEngine;

public class ToPierContextElement : ContextElement
{
    private Boat boat;

    protected override void Subscribe()
    {
        base.Subscribe();

        Boat.OnBoatStateEntered += OnBoatStateEntered;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        Boat.OnBoatStateEntered -= OnBoatStateEntered;
    }

    protected override void OnButtonClicked()
    {
        if (!boat) return;

        boat.SetState(BoatStateEnum.MovingToDock);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (!boat) return false;

        if (boat.CurrentStateEnum == BoatStateEnum.Idle) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.MovingToDock) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.UnloadingLoot) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.FloatingAway) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.Demolished) return false;

        return true;
    }

    protected override bool ShouldEnableButton()
    {
        if (!boat) return false;
        if (boat.Inventory.Items.Count <= 0) return false;

        var rider = boat.CurrentRider;
        if (!rider) return false;

        var citizen = rider.GetComponent<Citizen>();
        if (!citizen) return false;
        if (!citizen.IsCitizenAvailable()) return false;

        return true;
    }

    private void OnBoatStateEntered(Boat boat)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
        UpdateButtonEnabled();
    }
}