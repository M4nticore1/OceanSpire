using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    [SerializeField] private bool autoCleaning = false;

    [SerializeField] private bool useAmountLimit = true;
    public bool UseAmountLimit => useAmountLimit;

    [SerializeField] private bool useWeightLimit = false;
    public bool UseWeightLimit => useWeightLimit;

    [SerializeField] private float weightLimit = 0f;
    public float WeightLimit => weightLimit;

    [SerializeField] private float currentWeight = 0f;
    public float CurrentWeight => currentWeight;

    public float RemainingWeight => WeightLimit - CurrentWeight;

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    private Dictionary<int, ItemInstance> itemsDict = new();
    private Dictionary<ItemStackEnum, ItemStack> itemStacks = new();

    public event Action<ItemInstance> OnAddedItemAmount;
    public event Action<ItemInstance> OnRemovedItemAmount;

    public event Action<StorageItem> OnAddedMaxItemAmount;
    public event Action<StorageItem> OnRemovedMaxItemAmount;

    public event Action<ItemInstance> OnItemAmountChanged;
    public event Action<StorageItem> OnChangedItemMaxAmount;

    private void Awake()
    {
        var stackValues = (ItemStackEnum[])Enum.GetValues(typeof(ItemStackEnum));

        foreach (var stackEnum in stackValues) {
            itemStacks.Add(stackEnum, new ItemStack(stackEnum));
        }
    }

    public void Init()
    {
        Init(InventoryData.Default() ?? new InventoryData());
    }

    public void Init(InventoryData inventoryData)
    {
        if (inventoryData == null) {
            Debug.LogError("InventoryData is not valid");
            Init();
            return;
        }

        var items = inventoryData.Items;

        if (items != null) {
            foreach (var itemData in inventoryData.Items) {
                AddItem(itemData.Id, itemData.Amount);
            }
        }
    }

    public void AddItem(ItemInstance item)
    {
        AddItem(item.Definition.ItemId, item.Amount);
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

        AddWeight(amount * item.Definition.Weight);
    }

    public void RemoveItem(int id, int amount)
    {
        var item = GetItemById(id);
        amount = Mathf.Max(0, amount);

        item.RemoveAmount(amount);

        if (autoCleaning && item.Amount <= 0) {
            RemoveItem(id);
        }

        RemoveWeight(amount * item.Definition.Weight);
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

    public ItemInstance TryGetItemByIndex(int index)
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

    private void AddWeight(float weight)
    {
        SetWeight(currentWeight + weight);
    }

    private void RemoveWeight(float weight)
    {
        if (items.Count > 0f) {
            SetWeight(currentWeight - weight);
        }
        else {
            SetWeight(0f);
        }
    }

    private void SetWeight(float weight)
    {
        currentWeight = Mathf.Max(0f, weight);
    }
}