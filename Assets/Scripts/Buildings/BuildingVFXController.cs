using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingVFXController : VFXController
{
    [SerializeField] private BuildingsLoader buildingsLoader;
    [SerializeField] private ParticleSystem constructionFinishedVFX;
    [SerializeField] private Vector3 spawnPositionOffset = new Vector3(0f, 2.5f, 0f);

    private List<Building> registeredBuildings = new();

    protected override void Subscribe()
    {
        base.Subscribe();

        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingConstructionFinished += RegisterBuilding;
        Building.OnBuildingUpgradeFinished += RegisterBuilding;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingConstructionFinished -= RegisterBuilding;
        Building.OnBuildingUpgradeFinished -= RegisterBuilding;
    }

    private void OnBuildingInited(Building building)
    {
        if (!buildingsLoader.IsLoaded) return;

        RegisterBuilding(building);
    }

    private void RegisterBuilding(Building building)
    {
        if (!building) return;
        if (registeredBuildings.Contains(building)) return;

        Instantiate(constructionFinishedVFX, building.transform.position + spawnPositionOffset, Quaternion.identity);

        registeredBuildings.Add(building);
        StartCoroutine(UnregisterBuildingCoroutine(building));
    }

    private IEnumerator UnregisterBuildingCoroutine(Building building)
    {
        if (!building) yield break;
        yield return null;

        registeredBuildings.Remove(building);
    }
}