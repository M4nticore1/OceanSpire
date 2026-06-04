using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot Containers List", menuName = "Lists/Loot Containers List")]
public class LootContainersList : ScriptableObject
{
    private static LootContainersList _instance;
    public static LootContainersList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<LootContainersList>("Lists/LootContainersList");
            }
            return _instance;
        }
    }

    [SerializeField] private DriftingLoot[] lootContainers;
    public DriftingLoot[] LootContainers => lootContainers;

    private Dictionary<int, DriftingLoot> driftingLootDict;

    public DriftingLoot GetDriftingLoot(int id)
    {
        TryInitDictionary();

        return driftingLootDict[id];
    }

    private void TryInitDictionary()
    {
        if (driftingLootDict != null) return;

        driftingLootDict = new();

        foreach (var loot in lootContainers) {
            driftingLootDict.Add((int)loot.Definition.Id, loot);
        }
    }
}
