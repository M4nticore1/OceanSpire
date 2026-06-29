using UnityEngine;

public class PierNavObstacle : MonoBehaviour
{
    [SerializeField] private NavMeshBuilder navMeshBuilder;
    [SerializeField] private MeshCollider meshCollider;

    private GameObject spawnedNavMeshModifier;

    private void OnEnable()
    {
        Building.OnBuildingInited += OnBuildingInited;
        ConstructionComponent.OnGlobalConstructionFinished += OnConstructionCompleted;
        UpgradeComponent.OnGlobalUpgradeCompleted += OnUpgradeCompleted;
    }

    private void OnDisable()
    {
        Building.OnBuildingInited -= OnBuildingInited;
        ConstructionComponent.OnGlobalConstructionFinished -= OnConstructionCompleted;
        UpgradeComponent.OnGlobalUpgradeCompleted -= OnUpgradeCompleted;
    }

    private void OnBuildingInited(Building building)
    {
        if (!building) {
            Debug.LogError("building is not valid");
            return;
        }

        TryUpdateMesh(building.gameObject);
    }

    private void OnConstructionCompleted(ConstructionComponent construction)
    {
        if (!construction) {
            Debug.LogError("construction is not valid");
            return;
        }

        TryUpdateMesh(construction.gameObject);
    }

    private void OnUpgradeCompleted(UpgradeComponent upgradeComponent)
    {
        if (!upgradeComponent) {
            Debug.LogError("upgradeComponent is not valid");
            return;
        }

        TryUpdateMesh(upgradeComponent.gameObject);
    }

    private void TryUpdateMesh(GameObject gameObject)
    {
        var pierModule = gameObject.GetComponent<PierModule>();
        if (!ShouldUpdateNavMesh(pierModule)) return;

        UpdateMesh(gameObject);
        DestroySpawnedNavMeshModifier();
        SpawnNavMeshModifier(pierModule);
    }

    private void UpdateMesh(GameObject gameObject)
    {
        var pier = gameObject.GetComponent<PierModule>();
        var pierLevelData = pier.LevelData as PierLevelData;

        meshCollider.sharedMesh = pierLevelData.Collision;
        navMeshBuilder.BakeNavMesh();
    }

    private void SpawnNavMeshModifier(PierModule pierModule)
    {
        var navMeshModifier = pierModule.CurrentPierLevelData.NavMeshModifierVolumePrefab;
        if (!navMeshModifier) {
            Debug.LogError("navMeshModifier is not valid");
            return;
        }

        spawnedNavMeshModifier = Instantiate(navMeshModifier, transform);
    }

    private void DestroySpawnedNavMeshModifier()
    {
        if (!spawnedNavMeshModifier) return;

        Destroy(spawnedNavMeshModifier.gameObject);
    }

    private bool ShouldUpdateNavMesh(PierModule pierModule)
    {
        if (!pierModule) return false;

        return true;
    }
}