using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public static class ItemDatabase
{
    //public static List<ItemData> items { get; private set; } = new List<ItemData>();
    //public static Dictionary<int, ItemData> itemsById { get; private set; } = new Dictionary<int, ItemData>();
    //public static Dictionary<string, ItemData> itemsByIdName { get; private set; } = new Dictionary<string, ItemData>();

    //public static void Load()
    //{
    //    itemsById.Clear();
    //    itemsByIdName.Clear();

    //    items = Resources.LoadAll<ItemData>("Items").ToList();

    //    foreach (ItemData data in items)
    //    {
    //        if (!itemsById.ContainsKey(data.ItemId))
    //            itemsById.Add(data.ItemId, data);
    //        else
    //            Debug.LogWarning($"Duplicate ItemId {data.ItemId} for item {data.name}");

    //        if (!itemsByIdName.ContainsKey(data.ItemIdName))
    //            itemsByIdName.Add(data.ItemIdName, data);
    //        else
    //            Debug.LogWarning($"Duplicate itemIdName {data.ItemIdName} for item {data.name}");
    //    }
    //}

    //public static ItemData GetItemData(int id, List<ItemData> itemsList = null)
    //{
    //    return GetItemData_Internal(id, itemsList);
    //}

    //public static ItemData GetItemData(int id, List<ItemInstance> itemsList = null)
    //{
    //    return GetItemData_Internal(id, itemsList.Select(a => a.ItemData).ToList());
    //}

    //private static ItemData GetItemData_Internal(int id, List<ItemData> itemsList = null)
    //{
    //    itemsList ??= items;

    //    if (itemsList.Count > id && itemsList[id].ItemId == id)
    //    {
    //        return itemsList[id];
    //    }
    //    else
    //    {
    //        int currentId = 0;

    //        for (int i = 0; i < itemsList.Count; i++)
    //        {
    //            if (itemsList[i].ItemId == id)
    //            {
    //                currentId = i;
    //                return itemsList[i];
    //            }
    //        }

    //        return null;
    //    }
    //}

    //public static ItemInstance GetItem(int id, List<ItemInstance> itemsList = null)
    //{
    //    return GetItem_Internal(id, itemsList);
    //}

    //private static ItemInstance GetItem_Internal(int id, List<ItemInstance> itemsList = null)
    //{
    //    return itemsList[GetItemIndex(id, itemsList)];
    //}

    ////public static ItemData GetItem(string idName, List<ItemData> itemsList = null)
    ////{
    ////    itemsList ??= items;

    ////    int id = 0;

    ////    for (int i = 0; i < itemsList.Count; i++)
    ////    {
    ////        if (itemsList[i].itemIdName == idName)
    ////        {
    ////            id = i;
    ////            return itemsList[i];
    ////        }
    ////    }

    ////    return null;
    ////}

    //public static int GetItemIndex(int id, List<ItemData> itemsList = null)
    //{
    //    return GetItemIndex_Internal(id, itemsList);
    //}

    //public static int GetItemIndex(int id, List<ItemInstance> itemsList = null)
    //{
    //    return GetItemIndex_Internal(id, itemsList.Select(a => a.ItemData).ToList());
    //}

    //private static int GetItemIndex_Internal(int id, List<ItemData> itemsList = null)
    //{
    //    itemsList ??= items;

    //    if (itemsList.Count > id && itemsList[id].ItemId == id)
    //    {
    //        return id;
    //    }
    //    else
    //    {
    //        int currentId = 0;

    //        for (int i = 0; i < itemsList.Count; i++)
    //        {
    //            if (itemsList[i].ItemId == id)
    //            {
    //                currentId = i;
    //                return i;
    //            }
    //        }

    //        return -1;
    //    }
    //}

    //public static int GetItemIndex(string idName, List<ItemData> itemsList = null)
    //{
    //    itemsList ??= items;

    //    int id = 0;

    //    for (int i = 0; i < itemsList.Count; i++)
    //    {
    //        if (itemsList[i].ItemIdName == idName)
    //        {
    //            id = i;
    //            return i;
    //        }
    //    }

    //    return -1;
    //}
}
