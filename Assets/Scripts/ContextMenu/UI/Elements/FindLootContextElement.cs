using UnityEngine;

public class FindLootContextElement : ContextElement
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
        
        boat.UpdateState();
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

        var state = boat.CurrentStateEnum;
        if (state == BoatStateEnum.UnloadingLoot) return false;

        var rider = boat.CurrentRider;
        if (!rider) return false;

        var citizen = rider.GetComponent<Citizen>();
        if (!citizen) return false;
        if (!citizen.IsCitizenAvaliable()) return false;

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