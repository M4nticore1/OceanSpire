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

    [SerializeField] private LootContainer[] lootContainers;
    public LootContainer[] LootContainers => lootContainers;
}
