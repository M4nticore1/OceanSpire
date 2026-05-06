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

    public ItemStack(ItemStackEnum stackEnum)
    {
        this.stackEnum = stackEnum;
    }

    public void AddLimit(int value)
    {
        amount += value;
    }

    public void RemoveLimit(int value)
    {
        amount -= value;
    }

    public void AddAmount(int value)
    {
        TotalAmount += value;
    }

    public void RemoveAmount(int value)
    {
        TotalAmount -= value;
    }
}