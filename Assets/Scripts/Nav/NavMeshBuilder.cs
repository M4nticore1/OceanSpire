using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    public Coroutine bakeNavMeshCoroutine { get; private set; } = null;
    public bool isNavMeshBuilt { get; private set; } = false;

    public void BakeNavMesh()
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
}