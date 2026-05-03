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

    [SerializeField] private ItemData[] items;
    public ItemData[] Items => items;
    private Dictionary<int, ItemData> itemsDict = new();

    private void Init()
    {
        foreach (var item in items) {
            itemsDict.Add(item.ItemId, item);
        }
    }

    public ItemData GetItem(int id)
    {
        ItemData definition;
        itemsDict.TryGetValue(id, out definition);

        return definition;
    }
}