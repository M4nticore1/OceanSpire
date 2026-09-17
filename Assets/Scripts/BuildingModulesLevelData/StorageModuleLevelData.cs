using UnityEngine;

[CreateAssetMenu(fileName = "StorageBuildingLevelData", menuName = "Scriptable Objects/StorageBuildingLevelData")]
public class StorageModuleLevelData : BuildingModuleLevelData
{
    [SerializeField] private ItemStackInstance[] stacks;
    public ItemStackInstance[] Stacks => stacks;

    [SerializeField] private float raidLossRate = 0.0f;
    public float RaidLossRate => raidLossRate;

    public ItemStackInstance GetStack(ItemStackId stackId)
    {
        foreach (var stack in stacks) {
            if (stack == null) continue;
            if (stack.Definition == null) continue;
            if (stack.Definition.StackId != stackId) continue;

            return stack;
        }

        return null;
    }
}