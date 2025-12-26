using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LootList", menuName = "GameContent/LootList")]
public class LootList : ScriptableObject
{
    [SerializeField] private List<ItemData> loot = new List<ItemData>();
    public List<ItemData> Loot => loot;
    public Dictionary<int, ItemData> lootById { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<string, ItemData> lootByIdName { get; private set; } = new Dictionary<string, ItemData>();

    //public static void Load()
    //{
    //    lootById.Clear();
    //    lootByIdName.Clear();

    //    loot = Resources.LoadAll<ItemData>("Items").ToList();

    //    foreach (ItemData data in loot)
    //    {
    //        if (!lootById.ContainsKey(data.ItemId))
    //            lootById.Add(data.ItemId, data);
    //        else
    //            Debug.LogWarning($"Duplicate ItemId {data.ItemId} for item {data.name}");

    //        if (!lootByIdName.ContainsKey(data.ItemIdName))
    //            lootByIdName.Add(data.ItemIdName, data);
    //        else
    //            Debug.LogWarning($"Duplicate itemIdName {data.ItemIdName} for item {data.name}");
    //    }
    //}

    public ItemData GetItemData(int id, List<ItemData> itemsList = null)
    {
        return GetItemData_Internal(id, itemsList);
    }

    public ItemData GetItemData(int id, List<ItemInstance> itemsList)
    {
        return GetItemData_Internal(id, itemsList.Select(a => a.ItemData).ToList());
    }

    private ItemData GetItemData_Internal(int id, List<ItemData> itemsList = null)
    {
        itemsList ??= loot;

        if (itemsList.Count > id && itemsList[id].ItemId == id)
        {
            return itemsList[id];
        }
        else
        {
            int currentId = 0;

            for (int i = 0; i < itemsList.Count; i++)
            {
                if (itemsList[i].ItemId == id)
                {
                    currentId = i;
                    return itemsList[i];
                }
            }

            return null;
        }
    }

    public ItemInstance GetItem(int id, List<ItemInstance> itemsList = null)
    {
        return GetItem_Internal(id, itemsList);
    }

    private ItemInstance GetItem_Internal(int id, List<ItemInstance> itemsList = null)
    {
        return itemsList[GetItemIndex(id, itemsList)];
    }

    public int GetItemIndex(int id, List<ItemData> itemsList = null)
    {
        return GetItemIndex_Internal(id, itemsList);
    }

    public int GetItemIndex(int id, List<ItemInstance> itemsList = null)
    {
        return GetItemIndex_Internal(id, itemsList.Select(a => a.ItemData).ToList());
    }

    private int GetItemIndex_Internal(int id, List<ItemData> itemsList = null)
    {
        itemsList ??= loot;

        if (itemsList.Count > id && itemsList[id].ItemId == id)
        {
            return id;
        }
        else
        {
            int currentId = 0;

            for (int i = 0; i < itemsList.Count; i++)
            {
                if (itemsList[i].ItemId == id)
                {
                    currentId = i;
                    return i;
                }
            }

            return -1;
        }
    }

    public int GetItemIndex(string idName, List<ItemData> itemsList = null)
    {
        itemsList ??= loot;

        int id = 0;

        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].ItemIdName == idName)
            {
                id = i;
                return i;
            }
        }

        return -1;
    }
}
