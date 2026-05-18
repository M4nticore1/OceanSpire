using UnityEngine;

public class Wanderer : Human
{
    public bool IsRejected = false;
    public Vector3 SpawnPosition { get; private set; } = Vector3.zero;

    protected override void OnEnable()
    {
        base.OnEnable();

        CreaturesManager.Instance.RegisterWanderer(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        CreaturesManager.Instance.UnregisterWanderer(this);
    }

    protected override void OnInit(CreatureData data)
    {
        var wandererData = data as WandererData;

        IsRejected = wandererData.Rejected;
        SpawnPosition = wandererData.SpawnPosition.Vector3();

        SelectComponent.SetClickable(false);

        base.OnInit(data);
    }

    public void Reject()
    {
        IsRejected = true;
        BoatRider.SelectedBoat.FloatAway(SpawnPosition);
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        if (IsRejected) {
            boat.FloatAway(SpawnPosition);
        }
        else {
            BoatRider.SelectedBoat.SetState(BoatStateEnum.MovingToDock);
        }

        boat.ContextMenuTarget.SetShowContextMenu(false);
    }

    protected override void OnBoatStartedMoving(Boat boat)
    {
        base.OnBoatStartedMoving(boat);

        boat.SelectComponent.SetClickable(false);
    }

    protected override void OnBoatStoppedMoving(Boat boat)
    {
        base.OnBoatStoppedMoving(boat);

        boat.SelectComponent.SetClickable(true);
    }
}