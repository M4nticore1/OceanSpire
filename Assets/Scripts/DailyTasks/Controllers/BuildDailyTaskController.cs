using UnityEngine;

public class BuildDailyTaskController : DailyTaskController
{
    private BuildingsLoader buildingsLoader => BuildingsLoader.Instance;

    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;

        Building.OnBuildingInited += OnBuildingInited;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;

        Building.OnBuildingInited -= OnBuildingInited;

        return true;
    }

    private void OnBuildingInited(Building building)
    {
        if (buildingsLoader == null) return;
        if (!buildingsLoader.IsLoaded) return;

        AddTaskProgress(1);
    }
}