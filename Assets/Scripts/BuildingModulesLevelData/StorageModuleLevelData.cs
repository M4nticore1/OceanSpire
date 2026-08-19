using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageModuleLevelData : BuildingModuleLevelData
{
    [SerializeField] private ItemStack[] stacks;
    public ItemStack[] Stacks => stacks;

    [SerializeField] private float raidLossRate = 0.0f;
    public float RaidLossRate => raidLossRate;
}