using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class ItemCategoryData
{
    [SerializeField] private ItemCategory itemCategory;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private int amount;
    public int Amount => amount;
}

[System.Serializable]
public class ItemInstance
{
    [SerializeField] private ItemDefinition itemData;
    public ItemDefinition ItemData => itemData;
    [SerializeField] private int amount;
    public int Amount => amount;

    public ItemInstance(ItemDefinition itemData, int amount = 0)
    {
        this.itemData = itemData;
        this.amount = amount;
    }

    // Set Amount
    public int SetAmount(int amount, int maxAmount)
    {
        return this.amount = math.clamp(amount, 0, maxAmount);
    }

    public int SetAmount(int amount)
    {
        return this.amount = math.clamp(amount, 0, amount);
    }

    // Add Amount
    public int AddAmount(int amount)
    {
        return SetAmount(this.amount + amount);
    }

    // Remove Amount
    public int RemoveAmount(int amount)
    {
        return SetAmount(this.amount - amount);
    }
}