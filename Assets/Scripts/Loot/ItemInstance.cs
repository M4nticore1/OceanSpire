using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class ItemCategoryEntry
{
    public ItemCategory itemCategory;
    public int amount;

    public ItemCategoryEntry(ItemCategory itemCategory, int amount = 0)
    {
        this.itemCategory = itemCategory;
        this.amount = amount;
    }
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
    public int SetAmount(int amount, int maxAmount = 0)
    {
        //Debug.Log("SetAmount amount = " + amount);
        if (maxAmount == 0)
            maxAmount = amount;
        //Debug.Log("SetAmount maxAmount = " + maxAmount);

        int lastAmount = this.amount;
        this.amount = math.clamp(amount, 0, maxAmount);
        return math.abs(lastAmount - this.amount);
    }

    public int AddAmount(int amount, int maxAmount = 0)
    {
        return SetAmount(this.amount + amount, maxAmount);
    }

    public int SubtractAmount(int amount, int maxAmount = 0)
    {
        return SetAmount(this.amount - amount, maxAmount);
    }
}
