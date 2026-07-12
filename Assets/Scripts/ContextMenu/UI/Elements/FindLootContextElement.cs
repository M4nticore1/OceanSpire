using UnityEngine;

public class FindLootContextElement : ContextElement
{
    private Boat boat;

    protected override void Subscribe()
    {
        base.Subscribe();

        Boat.OnBoatStateEntered += OnBoatStateEntered;
        Boat.OnBoatStateExited += OnBoatStateEntered;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        Boat.OnBoatStateEntered -= OnBoatStateEntered;
        Boat.OnBoatStateExited -= OnBoatStateEntered;
    }

    protected override void OnButtonClicked()
    {
        if (!boat) return;

        boat.SetState(BoatStateEnum.FindingLoot);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (!boat) return false;

        var citizen = boat.CurrentRider?.GetComponent<Citizen>();
        if (!citizen) return false;
        if (!citizen.ShouldBoatFindLoot()) return false;

        if (boat.CurrentStateEnum == BoatStateEnum.MovingToDock) return true;

        return false;
    }

    private void OnBoatStateEntered(Boat boat)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
    }
}