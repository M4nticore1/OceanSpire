using UnityEngine;

public class UpgradeBuildingDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        Building.OnBuildingUpgradeFinished += HandleBuildingUpgradeFinished;

        return true;
    }

    protected override bool Unsubscribe()
    {
        Building.OnBuildingUpgradeFinished -= HandleBuildingUpgradeFinished;

        return true;
    }

    private void HandleBuildingUpgradeFinished(Building building)
    {
        if (building == null) return;

        AddTaskProgress(1);
    }
}