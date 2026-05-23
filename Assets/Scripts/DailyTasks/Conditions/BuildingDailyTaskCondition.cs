using UnityEngine;

public class BuildingDailyTaskCondition : DailyTaskCondition
{
    protected override bool Subscribe()
    {
        Building.OnBuildingInited += OnBuildingInited;

        return true;
    }

    protected override bool Unsubscribe()
    {
        Building.OnBuildingInited -= OnBuildingInited;

        return true;
    }

    private void OnBuildingInited(Building building)
    {
        if (!BuildingsLoader.Instance) return;
        if (!BuildingsLoader.Instance.IsLoaded) return;

        InvokeProgressChanged(1);
    }
}