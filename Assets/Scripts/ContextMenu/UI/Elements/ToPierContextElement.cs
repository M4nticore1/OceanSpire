using UnityEngine;

public class ToPierContextElement : ContextElement
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

        boat.SetState(BoatStateEnum.MovingToDock);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        boat = target.GetComponent<Boat>();
        if (!boat) return false;

        if (boat.CurrentStateEnum == BoatStateEnum.Idle) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.MovingToDock) return false;
        if (boat.CurrentStateEnum == BoatStateEnum.UnloadingLoot) return false;

        return true;
    }

    private void OnBoatStateEntered(Boat boat)
    {
        if (!boat) return;
        if (boat != this.boat) return;

        UpdateActive(boat.ContextMenuTarget);
    }
}