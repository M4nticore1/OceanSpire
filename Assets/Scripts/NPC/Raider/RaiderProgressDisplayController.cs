using UnityEngine;

public class RaiderProgressDisplayController : ProgressDisplayController
{
    [SerializeField] private Raider raider;

    private bool IsRaiding = false;

    protected override void Subscribe()
    {
        raider.OnRaidBuildingStarted += OnRaidBuildingStarted;
        raider.OnRaidBuildingStopped += OnRaidBuildingStopped;
    }

    protected override void Unsubscribe()
    {
        raider.OnRaidBuildingStarted -= OnRaidBuildingStarted;
        raider.OnRaidBuildingStopped -= OnRaidBuildingStopped;
    }

    public override void Tick()
    {
        base.Tick();

        if (IsRaiding) {
            ProgressDisplay.SetProgress(raider.GetProgress());
        }
    }

    private void OnRaidBuildingStarted(Building building)
    {
        IsRaiding = true;
        ProgressDisplay.Show();
    }

    private void OnRaidBuildingStopped(Building building)
    {
        IsRaiding = false;
        ProgressDisplay.Hide();
    }
}