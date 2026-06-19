using Unity.AI.Navigation;
using UnityEngine;

[CreateAssetMenu(fileName = "PierLevelData", menuName = "Modules Level Data/PierLevelData")]
public class PierLevelData : BuildingModuleLevelData
{
    [SerializeField] private int boatsCount = 0;
    public int BoatsCount => boatsCount;

    [SerializeField] private Mesh collision;
    public Mesh Collision => collision;

    [SerializeField] private GameObject navMeshModifierVolumePrefab;
    public GameObject NavMeshModifierVolumePrefab => navMeshModifierVolumePrefab;
}