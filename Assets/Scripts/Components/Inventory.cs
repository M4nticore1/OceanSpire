using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private bool autoCleaning = false;

    [SerializeField] private bool useAmountLimit = true;
    public bool UseAmountLimit => useAmountLimit;

    [SerializeField] private bool useWeightLimit = false;
    public bool UseWeightLimit => useWeightLimit;

    [SerializeField] private float weightLimit = 0;
    public float WeightLimit => weightLimit;

    private float currentWeight = 0;
    public float CurrentWeight => currentWeight;

    public float RemainingWeight => WeightLimit - CurrentWeight;

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    private Dictionary<int, ItemInstance> itemsDict = new();
    private Dictionary<ItemStackEnum, ItemStack> itemStacks = new();

    public event Action<ItemInstance> onAddedItemAmount;
    public event Action<ItemInstance> onRemovedItemAmount;

    public event Action<StorageItem> onAddedMaxItemAmount;
    public event Action<StorageItem> onRemovedMaxItemAmount;

    public event Action<ItemInstance> onItemAmountChanged;
    public event Action<StorageItem> onChangedItemMaxAmount;

    private void Awake()
    {
        var stackValues = (ItemStackEnum[])Enum.GetValues(typeof(ItemStackEnum));

        foreach (var stackEnum in stackValues) {
            itemStacks.Add(stackEnum, new ItemStack(stackEnum));
        }
    }

    public void AddItem(int id, int amount)
    {
        var item = GetItemById(id);
        if (item == null) {
            item = AddItem(id);
        }

        var stack = GetStack(item.Definition.Stack);
        if (useAmountLimit) {
            amount = math.clamp(amount, 0, stack.Amount - stack.GetItemAmountsSum());
        }

        if (useWeightLimit) {
            float remainingWeight = weightLimit - currentWeight;
            amount = math.clamp(amount, 0, (int)(remainingWeight / item.Definition.Weight));
        }

        item.AddAmount(amount);
        stack.AddItemAmount(item);

        currentWeight += amount * item.Definition.Weight;
    }

    public void RemoveItem(int id, int amount)
    {
        var item = GetItemById(id);
        amount = math.clamp(amount, 0, item.Amount);

        item.RemoveAmount(amount);

        if (autoCleaning && item.Amount <= 0) {
            RemoveItem(id);
        }

        currentWeight -= amount * item.Definition.Weight;
    }

    public void AddLimit(ItemStackEnum stack, int amount)
    {
        GetStack(stack).AddLimit(amount);
    }

    public void RemoveLimit(ItemStackEnum stack, int amount)
    {
        GetStack(stack).RemoveLimit(amount);
    }

    public int GetLimit(ItemStackEnum stack)
    {
        return GetStack(stack).Amount;
    }

    public ItemStack GetStack(ItemStackEnum stack)
    {
        return itemStacks[stack];
    }

    public ItemInstance GetItemById(int id)
    {
        itemsDict.TryGetValue(id, out var item);

        return item;
    }

    public ItemInstance GetItemByIndex(int index)
    {
        if (index >= items.Count) return null;

        return items[index];
    }

    private ItemInstance AddItem(int id)
    {
        var definition = ItemsList.Instance.GetItem(id);
        var item = new ItemInstance(definition);

        item.SetStack(GetStack(item.Definition.Stack));

        if (itemsDict.TryAdd(id, item)) {
            items.Add(item);
        }

        return item;
    }

    private ItemInstance RemoveItem(int id)
    {
        ItemInstance item = GetItemById(id);
        items.Remove(item);
        itemsDict.Remove(id);

        return item;
    }
}