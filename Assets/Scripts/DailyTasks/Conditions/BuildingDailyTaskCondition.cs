using UnityEngine;

public class BuildingDailyTaskCondition : DailyTaskCondition
{
    protected override bool Subscribe()
    {
        Building.onBuildingInited += OnBuildingInited;

        return true;
    }

    protected override bool Unsubscribe()
    {
        Building.onBuildingInited -= OnBuildingInited;

        return true;
    }

    private void OnBuildingInited(Building building)
    {
        if (!BuildingsLoader.Instance) return;
        if (!BuildingsLoader.Instance.IsLoaded) return;

        InvokeProgressChanged(1);
    }
}