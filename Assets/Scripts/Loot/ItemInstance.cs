using System;
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

    public ItemInstance(ItemData itemData, int amount = 0)
    {
        this.itemData = itemData;
        this.amount = amount;
        //this.maxAmount = maxAmount;
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
            this.amount += Math.Clamp(amount, 0, maxAmount);
    }

    public void SubtractAmount(int amount)
    {
        SetAmount(this.amount -= amount);
    }
}
