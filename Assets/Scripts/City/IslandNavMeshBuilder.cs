using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class IslandNavMeshBuilder : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface = null;

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    public bool isNavMeshBuilt { get; private set; } = false;

    private void OnEnable()
    {
        Building.onBuildingInited += OnBuildingInited;
        Building.onBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.onBuildingDemolished -= OnBuildingDemolished;
    }

    private void OnBuildingInited(Building building)
    {
        TryBake(building);
    }

    private void OnBuildingConstructionFinished(Building building)
    {
        TryBake(building);
    }

    private void OnBuildingDemolished(Building building)
    {
        TryBake(building);
    }

    private void TryBake(Building building)
    {
        if (!ShouldBake(building)) return;

        Bake();
    }

    private void Bake()
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
        GroundBuilding groundBuilding = building as GroundBuilding;
        if (!groundBuilding) return false;
        return true;
    }
}
