using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class CityNavMeshBuilder : MonoBehaviour
{
    [SerializeField] private NavMeshSurface towerNavMeshSurface = null;

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    public bool isNavMeshBuilt { get; private set; } = false;

    private void OnEnable()
    {
        EventBus.onBuildingPlaced += OnBuildingPlaced;
    }

    private void OnDisable()
    {
        EventBus.onBuildingPlaced -= OnBuildingPlaced;
    }

    private void Start()
    {
        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private void OnBuildingPlaced(Building building)
    {
        bakeNavMeshCoroutine = StartCoroutine(BakeNavMeshSurfaceCoroutine());
    }

    private IEnumerator BakeNavMeshSurfaceCoroutine()
    {
        if (bakeNavMeshCoroutine != null) yield break;

        yield return new WaitForEndOfFrame();
        towerNavMeshSurface.BuildNavMesh();
        bakeNavMeshCoroutine = null;
        EventBus.InvokNavMeshBaked();
    }
}
