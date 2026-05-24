using Unity.AI.Navigation;
using UnityEngine;

public class PierNavObstacle : MonoBehaviour
{
    [SerializeField] private NavMeshBuilder navMeshBuilder;
    [SerializeField] private MeshCollider meshCollider;

    private void OnEnable()
    {
        Building.OnBuildingInited += OnBuildingInited;
        ConstructionComponent.OnGlobalConstructionCompleted += OnConstructionCompleted;
        UpgradeComponent.OnGlobalUpgradeCompleted += OnUpgradeCompleted;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        ConstructionComponent.OnGlobalConstructionCompleted -= OnConstructionCompleted;
        UpgradeComponent.OnGlobalUpgradeCompleted -= OnUpgradeCompleted;
    }

    private void OnBuildingInited(Building building)
    {
        TryUpdateMesh(building.gameObject);
    }

    private void OnConstructionCompleted(ConstructionComponent construction)
    {
        TryUpdateMesh(construction.gameObject);
    }

    private void OnUpgradeCompleted(UpgradeComponent upgradeComponent)
    {
        TryUpdateMesh(upgradeComponent.gameObject);
    }

    private void TryUpdateMesh(GameObject gameObject)
    {
        if (!ShouldUpdateMesh(gameObject)) return;

        UpdateMesh(gameObject);
    }

    private void UpdateMesh(GameObject gameObject)
    {
        var pier = gameObject.GetComponent<PierModule>();
        var pierLevelData = pier.LevelData as PierLevelData;

        meshCollider.sharedMesh = pierLevelData.Collision;
        navMeshBuilder.BakeNavMesh();
    }

    private bool ShouldUpdateMesh(GameObject gameObject)
    {
        var pier = gameObject.GetComponent<PierModule>();

        return pier;
    }
}