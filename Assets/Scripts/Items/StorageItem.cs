using Unity.Mathematics;
using UnityEngine;

public class StorageItem
{
    public ItemInstance item { get; private set; } = null;

    public int maxAmount { get; private set; } = 0;

    public StorageItem(ItemInstance item, int maxAmount)
    {
        this.item = item;
        this.maxAmount = maxAmount;
    }

    public StorageItem(ItemInstance item)
    {
        this.item = item;
    }

    public void AddAmount(int value)
    {
        int amount = value;
        amount = math.clamp(amount, 0, maxAmount);
        item.AddAmount(amount);
    }

    public void RemoveAmount(int amount)
    {
        item.RemoveAmount(amount);
    }

    public void AddMaxAmount(int value)
    {
        maxAmount += value;
    }

    public void RemoveMaxAmount(int value)
    {
        maxAmount -= value;
        maxAmount = math.max(0, maxAmount);
    }
}
