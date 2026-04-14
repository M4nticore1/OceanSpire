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
        Building.onBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.onBuildingInited -= OnBuildingInited;
        Building.onBuildingDemolished -= OnBuildingDemolished;
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
