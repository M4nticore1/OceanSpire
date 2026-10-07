using UnityEngine;

public class FlyingLootDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        FlyingDriftingLoot.OnFlyingLootStartedFalling += HandleLootStartedFalling;

        return true;
    }

    protected override bool Unsubscribe()
    {
        FlyingDriftingLoot.OnFlyingLootStartedFalling += HandleLootStartedFalling;

        return true;
    }

    private void HandleLootStartedFalling(FlyingDriftingLoot driftingLoot)
    {
        if (driftingLoot == null) return;

        AddTaskProgress(1);
    }
}