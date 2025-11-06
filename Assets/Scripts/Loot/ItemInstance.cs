using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ItemCategoryEntry
{
    public ItemCategory itemCategory;
    public int amount;
}

[System.Serializable]
public class ItemInstance
{
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;
    [SerializeField] private int amount;
    public int Amount => amount;

    public ItemInstance(int itemId, int amount = 0)
    {
        itemData = ItemDatabase.GetItemData(itemId, (List<ItemData>)null);
        this.amount = amount;
    }

    public ItemInstance(ItemData itemData, int amount = 0)
    {
        this.itemData = itemData;
        this.amount = amount;
    }

    // Amount
    public void SetAmount(int amount)
    {
        this.amount = amount;
    }

    public void AddAmount(int amount, int maxAmount = 0)
    {
        if (maxAmount == 0)
            this.amount += amount;
        else if(maxAmount > 0)
            this.amount += math.clamp(amount, 0, maxAmount);
    }

    public int SubtractAmount(int amount, int maxAmount = 0)
    {
        if(maxAmount == 0)
            maxAmount = amount;

        int newAmount = math.clamp(amount, 0, maxAmount);
        SetAmount(newAmount);
        return newAmount;
    }
}
