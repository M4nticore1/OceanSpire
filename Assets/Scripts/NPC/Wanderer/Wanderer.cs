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

        var ridingBoat = BoatRider.RidingBoat;
        if (!ridingBoat) {
            Debug.LogError($"[{nameof(Wanderer)}] Riding Boat is not valid!");
            Destroy(gameObject);
        }
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

    protected override void BoatFloatAway()
    {
        base.BoatFloatAway();

        BoatRider.RidingBoat.FloatAway(SpawnPosition);
    }

    public override bool ShouldBoatMoveToDock()
    {
        if (!base.ShouldBoatMoveToDock()) return false;
        if (IsRejected) return false;

        return true;
    }

    public override bool ShouldBoatFloatAway()
    {
        if (!base.ShouldBoatFloatAway()) return false;
        if (!IsRejected) return false;

        return true;
    }

    public override bool ShouldStartExitingBoat()
    {
        return false;
    }

    protected override void HandleEnteredBoat(Boat boat)
    {
        base.HandleEnteredBoat(boat);

        UpdateRidingBoatClickable();
        boat.ContextMenuTarget.SetShowContextMenu(false);
    }

    protected override void OnBoatSetedIdle(Boat boat)
    {
        base.OnBoatSetedIdle(boat);

        UpdateRidingBoatClickable();
    }

    public override bool ShouldClick()
    {
        if (!base.ShouldClick()) return false;

        var ridingBoat = BoatRider.RidingBoat;
        if (!ridingBoat) return false;

        if (ridingBoat.Movement.IsMoving) return false;

        return true;
    }

    protected override void OnClick()
    {
        base.OnClick();

        var ridingBoat = BoatRider.RidingBoat;

        if (ridingBoat) {
            ridingBoat.SelectComponent.Click();
        }
        else {
            SelectComponent.Click();
        }
    }

    public void Accept()
    {
        IsAccepted = true;
        DetermineNextAction();
    }

    public void Reject()
    {
        IsRejected = true;
        RemoveBoatDock();
        DetermineNextAction();
    }

    private void RemoveBoatDock()
    {
        var ridingBoat = BoatRider.RidingBoat;
        if (!ridingBoat) {
            Debug.Log($"Riding Boat not fount at {name}");
            return;
        }

        ridingBoat.RemoveDockPoint();
    }

    private void UpdateRidingBoatClickable()
    {
        var ridingBoat = BoatRider.RidingBoat;
        if (ridingBoat == null) return;
        if (ridingBoat.CurrentStateEnum != BoatStateEnum.Idle) return;

        ridingBoat.SetClickable(true);
    }
}