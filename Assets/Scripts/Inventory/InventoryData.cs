using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryData
{
    public ItemData[] Items = new ItemData[0];

    public static InventoryData Default()
    {
        return new InventoryData();
    }

    public static InventoryData Create(Inventory inventory)
    {
        var itemsData = new List<ItemData>();

        foreach (var item in inventory.Items) {
            var itemData = ItemData.Create(item);
            itemsData.Add(itemData);
        }

        return new InventoryData()
        {
            Items = itemsData.ToArray()
        };
    }
}