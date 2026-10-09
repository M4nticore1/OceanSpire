using UnityEngine;

public class UpgradeBuildingDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        Building.OnBuildingUpgradeFinished += HandleBuildingUpgradeFinished;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        Building.OnBuildingUpgradeFinished -= HandleBuildingUpgradeFinished;

        return true;
    }

    private void HandleBuildingUpgradeFinished(Building building)
    {
        if (building == null) return;

        AddTaskProgress(1);
    }
}