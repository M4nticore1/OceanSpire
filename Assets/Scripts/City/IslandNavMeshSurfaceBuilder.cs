using UnityEngine;

public class IslandNavMeshSurfaceBuilder : MonoBehaviour
{
    [SerializeField] private NavMeshBuilder navMeshBuilder;

    private void OnEnable()
    {
        Building.OnBuildingInited += OnBuildingInited;
        Building.OnBuildingConstructionFinished += OnBuildingConstructionFinished;
        Building.OnBuildingDemolished += OnBuildingDemolished;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        Building.OnBuildingConstructionFinished -= OnBuildingConstructionFinished;
        Building.OnBuildingDemolished -= OnBuildingDemolished;
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

        navMeshBuilder.BakeNavMesh();
    }

    private bool ShouldBake(Building building)
    {
        var groundBuilding = building as GroundBuilding;

        return groundBuilding;
    }
}
