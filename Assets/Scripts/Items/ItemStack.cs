using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum ItemStackEnum
{
    Population,
    Electricity,
    Food,
    Wood,
    Stone,
    Scrap,
    Plastic,
    Provisions,
    Weapon,
}

[System.Serializable]
public class ItemStack : IItemAmount
{
    [SerializeField] private ItemStackEnum stackEnum = ItemStackEnum.Population;
    public ItemStackEnum StackEnum => stackEnum;

    [SerializeField, FormerlySerializedAs("limit")] private int amount = 0;
    public int Amount => amount;

    public List<IItemAmount> ItemAmounts = new();

    public event Action<int> OnAmountChanged;

    public ItemStack(ItemStackEnum stackEnum)
    {
        this.stackEnum = stackEnum;
    }

    public void AddLimit(int value)
    {
        SetLimit(amount + value);
    }

    public void RemoveLimit(int value)
    {
        value = Mathf.Clamp(value, 0, amount);
        SetLimit(amount - value);
    }

    public void AddItemAmount(IItemAmount value)
    {
        if (ItemAmounts.Contains(value)) return;

        ItemAmounts.Add(value);
    }

    public void RemoveItemAmount(IItemAmount value)
    {
        ItemAmounts.Remove(value);
    }

    public int GetItemAmountsSum()
    {
        int sum = 0;
        foreach (var item in ItemAmounts) {
            sum += item.Amount;
        }
        
        return sum;
    }

    private void SetLimit(int value)
    {
        amount = value;
        OnAmountChanged?.Invoke(value);
    }
}