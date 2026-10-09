using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorFrameModule : BuildingModule
{
    [SerializeField] NavMeshSurface navMeshSurface;

    // Building Places
    [SerializeField] private List<BuildingPlace> roomBuildingPlaces;
    public List<BuildingPlace> RoomBuildingPlaces => roomBuildingPlaces;

    [SerializeField] private BuildingPlace hallBuildingPlace;
    public BuildingPlace HallBuildingPlace => hallBuildingPlace;

    [SerializeField] private BuildingPlace floorBuildingPlace;
    public BuildingPlace FloorBuildingPlace => floorBuildingPlace;

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;

    public static event Action<FloorFrameModule> OnFloorModuleInited;

    protected override void Subscribe()
    {
        base.Subscribe();

        OwnedBuilding.OnDemolished += OnDemolished;

        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.OnBuildingLevelChanged += OnBuildingUpgraded;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();

        OwnedBuilding.OnDemolished -= OnDemolished;

        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingLevelChanged -= OnBuildingUpgraded;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
    }

    protected override void OnInit()
    {
        base.OnInit();

        int floorIndex = OwnedTowerBuilding.FloorIndex;
        BuildingsManager.Instance.RegisterFloorModule(this);

        InitBuildings();

        OnFloorModuleInited?.Invoke(this);
    }

    private void OnDemolished()
    {
        BuildingsManager.Instance.UnregisterFloorModule(this);
        DemolishBuildings();
    }

    private void OnBuildingInited(Building building)
    {
        if (!ShouldBake(building)) return;

        TryBake(building);
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldBake(building)) return;

        TryBake(building);
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        TryBake(building);
    }

    private void OnBuildingUpgraded(Building building)
    {
        TryBake(building);
    }

    private void InitBuildings()
    {
        floorBuildingPlace.Init(OwnedTowerBuilding.FloorIndex + 1);
        hallBuildingPlace.Init(OwnedTowerBuilding.FloorIndex);

        for (int i = 0; i < BuildingsManager.RoomsCountPerFloor; i++) {
            roomBuildingPlaces[i].Init(OwnedTowerBuilding.FloorIndex);
        }
    }

    private void DemolishBuildings()
    {
        foreach (var buildingPlace in roomBuildingPlaces) {
            var building = buildingPlace.PlacedBuilding;
            if (!building) continue;

            building.Demolish();
        }

        var floorBuilding = floorBuildingPlace.PlacedBuilding;
        if (floorBuilding) {
            floorBuilding.Demolish();
        }
    }

    private void TryBake(Building building)
    {
        if (!ShouldBake(building)) return;

        StartBaking();
    }

    private void StartBaking()
    {
        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        if (bakeNavMeshCoroutine != null) yield break;

        yield return new WaitForEndOfFrame();
        navMeshSurface.BuildNavMesh();
        bakeNavMeshCoroutine = null;

        EventBus.InvokeNavMeshBaked();
    }

    private bool ShouldBake(Building building)
    {
        if (OwnedTowerBuilding.FloorIndex <= 0) return false;

        var towerBuilding = building as TowerBuilding;
        if (!towerBuilding) return false;

        if (towerBuilding.FloorIndex != OwnedTowerBuilding.FloorIndex) return false;

        return true;
    }
}
