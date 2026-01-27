using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "lootList", menuName = "GameContent/lootList")]
public class ItemsList : ScriptableObject
{
    [SerializeField] private ItemData[] items;
    public ItemData[] Items => items;

    public ItemData GetItemData(int id, ItemData[] itemsList = null)
    {
        return GetItemData_Internal(id, itemsList);
    }

    public ItemData GetItemData(int id, ItemInstance[] itemsList)
    {
        return GetItemData_Internal(id, itemsList.Select(a => a.ItemData).ToArray());
    }

    private ItemData GetItemData_Internal(int id, ItemData[] itemsList = null)
    {
        itemsList ??= items;

        if (itemsList.Length > id && itemsList[id].ItemId == id)
        {
            return itemsList[id];
        }
        else
        {
            int currentId = 0;

            for (int i = 0; i < itemsList.Length; i++)
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

    public ItemInstance GetItem(int id, ItemInstance[] itemsList = null)
    {
        return GetItem_Internal(id, itemsList);
    }

    private ItemInstance GetItem_Internal(int id, ItemInstance[] itemsList = null)
    {
        return itemsList[GetItemIndex(id, itemsList)];
    }

    public int GetItemIndex(int id, ItemData[] itemsList = null)
    {
        return GetItemIndex_Internal(id, itemsList);
    }

    public int GetItemIndex(int id, ItemInstance[] itemsList = null)
    {
        return GetItemIndex_Internal(id, itemsList.Select(a => a.ItemData).ToArray());
    }

    private int GetItemIndex_Internal(int id, ItemData[] itemsList = null)
    {
        itemsList ??= items;

        if (itemsList.Length > id && itemsList[id].ItemId == id)
        {
            return id;
        }
        else
        {
            int currentId = 0;

            for (int i = 0; i < itemsList.Length; i++)
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

    public int GetItemIndex(string idName, ItemData[] itemsList = null)
    {
        itemsList ??= items;

        int id = 0;

        for (int i = 0; i < itemsList.Length; i++)
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
