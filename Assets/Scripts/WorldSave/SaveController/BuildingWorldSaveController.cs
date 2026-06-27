using UnityEngine;

public class BuildingWorldSaveController : WorldSaveController
{
    [SerializeField] private BuildingsLoader buildingsLoader;

    protected override void OnSubscribe()
    {
        base.OnSubscribe();

        Building.OnBuildingInited += OnBuildingInited;
    }

    protected override void OnUnsubscribe()
    {
        base.OnUnsubscribe();

        Building.OnBuildingInited -= OnBuildingInited;
    }

    private void OnBuildingInited(Building building)
    {
        if (!buildingsLoader.IsLoaded) return;

        SaveWorld();
    }
}