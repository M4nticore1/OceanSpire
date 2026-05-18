using System;
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

    public int TotalAmount { get; private set; } = 0;

    public event Action OnAmountChanged;

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

    public void AddAmount(int value)
    {
        SetAmount(TotalAmount + value);
    }

    public void RemoveAmount(int value)
    {
        SetAmount(TotalAmount - value);
    }

    private void SetLimit(int value)
    {
        amount = value;
        OnAmountChanged?.Invoke();
    }

    private void SetAmount(int value)
    {
        TotalAmount = value;
    }
}