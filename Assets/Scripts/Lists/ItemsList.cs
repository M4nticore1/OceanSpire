using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot List", menuName = "Game Content/Loot List")]
public class ItemsList : ScriptableObject
{
    private static ItemsList _instance;
    public static ItemsList Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Resources.Load<ItemsList>("Lists/ItemsList");
                _instance.Init();
            }
            return _instance;
        }
    }

    [SerializeField] private ItemDefinition[] items;
    public ItemDefinition[] Items => items;
    private Dictionary<ItemID, ItemDefinition> itemsDict = new();

    private void Init()
    {
        foreach (var item in items) {
            itemsDict.Add(item.ItemId, item);
        }
    }

    public ItemDefinition GetItem(ItemID id)
    {
        ItemDefinition definition;
        itemsDict.TryGetValue(id, out definition);

        return definition;
    }
}