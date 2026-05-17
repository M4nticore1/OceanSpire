using System;
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

    public event Action OnAmountChanged;

    public ItemInstance(ItemDefinition definition)
    {
        this.definition = definition;
    }

    public int SetAmount(int amount)
    {
        this.amount = amount;
        OnAmountChanged?.Invoke();

        return this.amount;
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