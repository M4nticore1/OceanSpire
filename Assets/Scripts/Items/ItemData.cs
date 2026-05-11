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
}