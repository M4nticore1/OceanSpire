using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public int Id = 0;
    public int Amount = 0;

    public static ItemData Create(ItemInstance item)
    {
        return new ItemData()
        {
            Id = item.Definition.ItemId,
            Amount = item.Amount
        };
    }

    public static ItemData[] Create(ItemInstance[] items)
    {
        var itemsData = new ItemData[items.Length];

        for (int i = 0; i < items.Length; i++) {
            itemsData[i] = Create(items[i]);
        }

        return itemsData;
    }
}