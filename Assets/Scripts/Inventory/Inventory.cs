using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, ILocalizable
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
    public int RemainingWeightInt => Mathf.RoundToInt(WeightLimit) - Mathf.RoundToInt(CurrentWeight);

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    private Dictionary<ItemID, ItemInstance> itemsDict = new();
    private Dictionary<ItemStackEnum, ItemStack> itemStacks = new();

    public event Action<ItemInstance> OnItemAdded;
    public event Action<ItemInstance> OnItemRemoved;

    public event Action<ItemInstance> OnItemAmountAdded;
    public event Action<ItemInstance> OnItemAmountRemoved;

    public event Action<StorageItem> OnItemLimitAdded;
    public event Action<StorageItem> OnItemLimitRemoved;

    public event Action<ItemInstance> OnItemAmountChanged;
    public event Action<StorageItem> OnItemLimitChanged;

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

    public void AddItem(ItemID id, int amount)
    {
        var item = GetItem(id);
        if (item == null) {
            item = AddItem(id);
        }

        var stack = GetStack(item.Definition.Stack);
        if (useAmountLimit) {
            amount = Mathf.Clamp(amount, 0, stack.Amount - stack.GetItemAmountsSum());
        }

        if (useWeightLimit) {
            float remainingWeight = weightLimit - currentWeight;
            amount = Mathf.Clamp(amount, 0, (int)(remainingWeight / item.Definition.Weight));
        }

        var startAmount = item.Amount;

        item.AddAmount(amount);
        stack.AddItemAmount(item);

        AddWeight(amount * item.Definition.Weight);

        if (item.Amount != startAmount) {
            if (amount > startAmount)
                OnItemAmountAdded?.Invoke(item);
            else
                OnItemAmountRemoved?.Invoke(item);

            OnItemAmountChanged?.Invoke(item);
        }
    }

    public void RemoveItem(ItemInstance item)
    {
        RemoveItem(item.Definition.ItemId, item.Amount);
    }

    public void RemoveItem(ItemID id, int amount)
    {
        var item = GetItem(id);
        amount = Mathf.Max(0, amount);

        var startAmount = item.Amount;

        item.RemoveAmount(amount);

        if (item.Amount <= 0 && autoCleaning) {
            RemoveItem(id);
        }

        RemoveWeight(amount * item.Definition.Weight);

        if (item.Amount != startAmount) {
            if (amount > startAmount)
                OnItemAmountAdded?.Invoke(item);
            else
                OnItemAmountRemoved?.Invoke(item);

            OnItemAmountChanged?.Invoke(item);
        }
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

    public ItemInstance GetItem(ItemID id)
    {
        itemsDict.TryGetValue(id, out var item);

        return item;
    }

    public ItemInstance TryGetItemByIndex(int index)
    {
        if (index >= items.Count) return null;

        return items[index];
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "weight", Mathf.RoundToInt(currentWeight).ToString("F0") },
            { "maxWeight", Mathf.RoundToInt(weightLimit).ToString("F0") }
        };
    }

    private ItemInstance AddItem(ItemID id)
    {
        var definition = ItemsList.Instance.GetItem(id);
        var item = definition.CreateInstance();

        item.SetStack(GetStack(item.Definition.Stack));

        if (itemsDict.TryAdd(id, item)) {
            items.Add(item);
        }

        OnItemAdded?.Invoke(item);

        return item;
    }

    private ItemInstance RemoveItem(ItemID id)
    {
        ItemInstance item = GetItem(id);
        items.Remove(item);
        itemsDict.Remove(id);

        OnItemRemoved?.Invoke(item);

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