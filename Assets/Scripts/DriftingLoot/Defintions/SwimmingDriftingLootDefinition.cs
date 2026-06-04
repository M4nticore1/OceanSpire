using UnityEngine;

[CreateAssetMenu(fileName = "swimming_loot_definition", menuName = "Drifting Loot/SwimmingDriftingLootDefinition")]
public class SwimmingDriftingLootDefinition : DriftingLootDefinition
{
    [Header("Swimming")]
    [SerializeField] private LootTableData[] lootTable;
    public LootTableData[] LootTable => lootTable;
}