using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

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

    protected override void Subscribe()
    {
        OwnedBuilding.onInited += OnInited;
        OwnedBuilding.onDemolished += OnDemolished;

        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    protected override void Unsubscribe()
    {
        OwnedBuilding.onInited -= OnInited;
        OwnedBuilding.onDemolished -= OnDemolished;

        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnInited()
    {
        int floorIndex = TowerOwnedBuilding.FloorIndex;
        BuildingsManager.Instance.RegisterFloorModule(this);
        InitBuildings();
    }

    private void OnDemolished()
    {
        BuildingsManager.Instance.UnregisterFloorModule(this);
        DemolishBuildings();
    }

    private void OnBuildingInited(Building building)
    {
        if (!ShouldBake(building)) return;
        StartBaking();
    }

    private void OnBuildingDemolished(Building building)
    {
        if (!ShouldBake(building)) return;

        StartBaking();
    }

    private void InitBuildings()
    {
        floorBuildingPlace.Init(TowerOwnedBuilding.FloorIndex + 1);
        hallBuildingPlace.Init(TowerOwnedBuilding.FloorIndex);

        for (int i = 0; i < BuildingsManager.RoomsCountPerFloor; i++) {
            roomBuildingPlaces[i].Init(TowerOwnedBuilding.FloorIndex);
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
        EventBus.InvokNavMeshBaked();
    }

    private bool ShouldBake(Building building)
    {
        TowerBuilding towerBuilding = building as TowerBuilding;
        if (!towerBuilding) return false;

        TowerBuilding ownedTowerBuilding = OwnedBuilding as TowerBuilding;

        if (towerBuilding.FloorIndex != ownedTowerBuilding.FloorIndex) return false;
        return true;
    }
}
