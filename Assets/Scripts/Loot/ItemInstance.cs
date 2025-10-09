using System;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public ItemData itemData;
    public int amount;
    public int maxAmount;

    public ItemInstance(ItemData itemData, int amount, int maxAmount)
    {
        this.itemData = itemData;
        this.amount = amount;
        this.maxAmount = maxAmount;
    }

    // Amount
    public void SetAmount(int newAmount)
    {
        amount = newAmount;
        amount = math.clamp(newAmount, 0, maxAmount);
    }

    public void AddAmount(int amount)
    {
        SetAmount(this.amount += amount);
    }

    public void SubtractAmount(int amount)
    {
        SetAmount(this.amount -= amount);
    }

    // Max Amount
    public void SetMaxAmount(int amount)
    {
        this.maxAmount = amount;
    }

    public void AddMaxAmount(int amount)
    {
        SetMaxAmount(this.maxAmount += amount);
    }

    public void SubtractMaxAmount(int amount)
    {
        SetMaxAmount(this.maxAmount -= amount);
    }
}
