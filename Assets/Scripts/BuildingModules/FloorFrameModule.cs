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

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onBuildingInited += OnBuildingInited;
        EventBus.onBuildingDemolished += OnBuildingDemolished;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onBuildingInited -= OnBuildingInited;
        EventBus.onBuildingDemolished -= OnBuildingDemolished;
    }

    protected override void OnInit()
    {
        int floorIndex = (OwnedBuilding as TowerBuilding).floorIndex;
        floorBuildingPlace.Init(floorIndex + 1);
        hallBuildingPlace.Init(floorIndex);

        for (int i = 0; i < BuildingsManager.RoomsCountPerFloor; i++) {
            roomBuildingPlaces[i].Init(floorIndex);
        }
    }

    protected override void OnBuildingStartWorking()
    {

    }

    protected override void OnBuildingStopWorking()
    {

    }

    protected override void OnEnterBuilding(EntityCityNavigator navigator)
    {

    }

    protected override void OnExitBuilding(EntityCityNavigator navigator)
    {

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

    private void StartBaking()
    {
        Debug.Log("StartBaking");
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

        if (towerBuilding.floorIndex != ownedTowerBuilding.floorIndex) return false;
        return true;
    }
}
