using UnityEngine;

[CreateAssetMenu(fileName = "flying_loot_definition", menuName = "Drifting Loot/FlyingDriftingLootDefinition")]
public class FlyingDriftingLootDefinition : DriftingLootDefinition
{
    [Header("Flying")]
    [SerializeField] private DriftingLoot[] demolishDriftingLootTable;
    public DriftingLoot[] DemolishDriftingLootTable => demolishDriftingLootTable;

    [SerializeField] private int floorsToSpawn = 0;
    public int FloorsToSpawn => floorsToSpawn;

    [SerializeField] private int minSpawnFloor = 0;
    public int MinSpawnFloor => minSpawnFloor;

    [SerializeField] private int maxSpawnFloor = 0;
    public int MaxSpawnFloor => maxSpawnFloor;
}