using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemCategoryData
{
    [SerializeField] private ItemCategory itemCategory;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private int amount;
    public int Amount => amount;
}

[Serializable]
public class ItemInstance : IItemAmount
{
    [SerializeField, FormerlySerializedAs("itemData")] private ItemDefinition definition;
    public ItemDefinition Definition => definition;

    [SerializeField] private int amount;
    public int Amount => amount;

    public ItemStack Stack { get; private set; }

    public event Action<int> OnAmountChanged;

    public ItemInstance(ItemDefinition definition)
    {
        this.definition = definition;
    }

    public int SetAmount(int amount)
    {
        this.amount = Mathf.Max(0, amount);
        OnAmountChanged?.Invoke(amount);

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

    public static ItemInstance Create(ItemData itemData)
    {
        var definition = ItemsList.Instance.GetItem(itemData.Id);

        var item = new ItemInstance(definition);
        item.SetAmount(itemData.Amount);

        return item;
    }

    public static ItemInstance[] Create(ItemData[] itemData)
    {
        if (itemData == null) {
            Debug.Log("itemData array not found");
            return null;
        }

        List<ItemInstance> items = new List<ItemInstance>();

        foreach (ItemData item in itemData) {
            items.Add(Create(item));
        }

        return items.ToArray();
    }
}