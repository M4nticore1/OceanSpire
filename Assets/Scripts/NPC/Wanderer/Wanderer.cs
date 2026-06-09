using UnityEngine;

public class Wanderer : Human
{
    public bool IsAccepted { get; private set; } = false;
    public bool IsRejected { get; private set; } = false;
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

    protected override void DetermineNextAction()
    {
        if (ShouldBoatMoveToDock()) {
            BoatMoveToDock();
            return;
        }
        if (ShouldBoatFloatAway()) {
            BoatFloatAway();
            return;
        }

        base.DetermineNextAction();
    }

    protected override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (IsRejected) return false;

        return true;
    }

    protected override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;
        if (!IsRejected) return false;

        return true;
    }

    public void Accept()
    {
        IsAccepted = true;
        DetermineNextAction();
    }

    public void Reject()
    {
        IsRejected = true;
        DetermineNextAction();
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        boat.ContextMenuTarget.SetShowContextMenu(false);
    }
}