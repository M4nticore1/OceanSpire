using Unity.Mathematics;
using UnityEngine;

public class StorageItem
{
    public ItemInstance item { get; private set; }
    public int maxAmount { get; private set; }
    public ItemInstance maxAmountItem { get; private set; }

    public StorageItem(ItemInstance item, int maxAmount)
    {
        this.item = item;
        this.maxAmount = maxAmount;
        maxAmountItem = new ItemInstance(item.Definition);
    }

    public StorageItem(ItemInstance item)
    {
        this.item = item;
        maxAmountItem = new ItemInstance(item.Definition);
    }

    public void AddAmount(int value)
    {
        int amount = value;
        amount = math.clamp(amount, 0, maxAmount - item.Amount);
        item.AddAmount(amount);
    }

    public void RemoveAmount(int amount)
    {
        item.RemoveAmount(amount);
    }

    public void AddMaxAmount(int value)
    {
        maxAmount += value;
        maxAmountItem.SetAmount(maxAmount);
    }

    public void RemoveMaxAmount(int value)
    {
        maxAmount -= value;
        maxAmount = math.max(0, maxAmount);
        maxAmountItem.SetAmount(maxAmount);
    }
}
