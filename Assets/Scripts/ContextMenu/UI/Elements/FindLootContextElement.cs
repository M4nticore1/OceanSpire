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
        if (boat.CurrentStateEnum != BoatStateEnum.MovingToDock) return;

        boat.SetState(BoatStateEnum.FindingLoot);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (!boat) return false;

        if (!boat.ShouldFindLoot()) return false;

        var state = boat.CurrentStateEnum;
        if (state == BoatStateEnum.MovingToDock) return true;

        return false;
    }

    private void OnBoatStateEntered(Boat boat)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
    }
}