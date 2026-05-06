using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ItemCategoryData
{
    [SerializeField] private ItemCategory itemCategory;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private int amount;
    public int Amount => amount;
}

[System.Serializable]
public class ItemInstance : IItemAmount
{
    [SerializeField, FormerlySerializedAs("itemData")] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    [SerializeField] private int amount;
    public int Amount => amount;

    public ItemStack Stack { get; private set; }

    public ItemInstance(ItemDefinition definition)
    {
        this.definition = definition;
    }

    public int SetAmount(int amount, int maxAmount)
    {
        return this.amount = math.clamp(amount, 0, maxAmount);
    }

    public int SetAmount(int amount)
    {
        return this.amount = math.clamp(amount, 0, amount);
    }

    public int AddAmount(int amount)
    {
        return SetAmount(this.amount + amount);
    }

    public int RemoveAmount(int amount)
    {
        return SetAmount(this.amount - amount);
    }

    public void SetStack(ItemStack stack)
    {
        this.Stack = stack;
    }
}