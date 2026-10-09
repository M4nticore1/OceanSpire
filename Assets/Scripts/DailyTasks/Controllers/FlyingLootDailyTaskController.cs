using UnityEngine;

public class FlyingLootDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        FlyingDriftingLoot.OnFlyingLootStartedFalling += HandleLootStartedFalling;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        FlyingDriftingLoot.OnFlyingLootStartedFalling += HandleLootStartedFalling;

        return true;
    }

    private void HandleLootStartedFalling(FlyingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;

        AddTaskProgress(1);
    }
}