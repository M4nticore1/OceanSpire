using System;
using System.Collections;
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

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    private Dictionary<ItemID, ItemInstance> itemsDict = new();
    private Dictionary<ItemStackEnum, ItemStack> itemStacks = new();

    private ItemsList itemsList => ItemsList.Instance;

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

    private void OnDestroy()
    {
        foreach (var item in items) {
            UnsubscribeItem(item);
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

        var itemsData = inventoryData.Items;

        if (itemsData != null) {
            foreach (var itemData in itemsData) {
                AddItemAmount(itemData.Id, itemData.Amount);
            }
        }
    }

    private ItemInstance AddItem(ItemID id)
    {
        var definition = GetItemDefinition(id);
        if (definition == null) return null;

        var item = definition.CreateInstance();
        AddItem(item);

        return item;
    }

    public void AddItem(ItemInstance item)
    {
        if (item == null) return;

        SubscribeItem(item);

        if (useAmountLimit) {
            item.SetStack(GetStack(item.Definition.Stack));
        }

        items.Add(item);
        itemsDict.Add(item.Definition.ItemId, item);

        OnItemAdded?.Invoke(item);
    }

    public void AddItemAmount(ItemInstance item)
    {
        AddItemAmount(item.Definition.ItemId, item.Amount);
    }

    public void AddItemAmount(ItemID id, int amount)
    {
        var definition = GetItemDefinition(id);
        if (definition == null) return;

        if (useAmountLimit) {
            var stack = GetStack(definition.Stack);
            var maxAmount = stack != null ? stack.Amount - stack.GetItemAmountsSum() : amount;
            amount = Mathf.Clamp(amount, 0, maxAmount);
        }

        if (useWeightLimit && definition.Weight > 0) {
            amount = Mathf.Clamp(amount, 0, (int)(GetRemainingWeight() / definition.Weight));
        }

        if (!ShouldAddItem(id, amount)) return;

        var item = GetInventoryItem(id) ?? AddItem(id);
        item.AddAmount(amount);
    }

    public void RemoveItem(ItemInstance item)
    {
        if (item == null) return;

        UnsubscribeItem(item);

        items.Remove(item);
        itemsDict.Remove(item.Definition.ItemId);

        OnItemRemoved?.Invoke(item);
    }

    public void RemoveItemAmount(ItemInstance item)
    {
        RemoveItemAmount(item.Definition.ItemId, item.Amount);
    }

    public void RemoveItemAmount(ItemID id, int amount)
    {
        var item = GetInventoryItem(id);
        if (item == null) return;

        item.RemoveAmount(amount);
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
        if (!itemStacks.TryGetValue(stack, out var itemStack)) {
            Debug.LogError($"[{nameof(Inventory)}] Stack ({stack}) is not valid!");
        }

        return itemStack;
    }

    public ItemInstance GetInventoryItem(ItemID id)
    {
        itemsDict.TryGetValue(id, out var item);
        return item;
    }

    public ItemInstance TryGetItemByIndex(int index)
    {
        if (index < 0 || index >= items.Count) return null;
        return items[index];
    }

    public float GetCurrentWeight()
    {
        var weight = 0f;
        for (int i = 0; i < items.Count; i++) {
            var item = items[i];
            if (item == null) continue;

            var definition = item.Definition;
            if (!definition) continue;

            weight += definition.Weight * item.Amount;
        }

        return weight;
    }

    public float GetRemainingWeight()
    {
        return WeightLimit - GetCurrentWeight();
    }

    public int GetRemainingWeightInt()
    {
        return Mathf.RoundToInt(WeightLimit) - Mathf.RoundToInt(GetCurrentWeight());
    }

    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "weight", Mathf.RoundToInt(GetCurrentWeight()).ToString("F0") },
            { "maxWeight", Mathf.RoundToInt(weightLimit).ToString("F0") }
        };
    }

    private void SubscribeItem(ItemInstance item)
    {
        if (item == null) return;

        item.OnItemAmountAdded += HandleItemAmountAdded;
        item.OnItemAmountRemoved += HandleItemAmountRemoved;
    }

    private void UnsubscribeItem(ItemInstance item)
    {
        if (item == null) return;

        item.OnItemAmountAdded -= HandleItemAmountAdded;
        item.OnItemAmountRemoved -= HandleItemAmountRemoved;
    }

    private void HandleItemAmountAdded(ItemInstance item, int amount)
    {
        var stack = GetStack(item.Definition.Stack);
        stack.AddItemAmount(item);

        OnItemAmountAdded?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);
    }

    private void HandleItemAmountRemoved(ItemInstance item, int amount)
    {
        var stack = GetStack(item.Definition.Stack);
        stack.RemoveItemAmount(item);

        OnItemAmountRemoved?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);

        if (item.Amount <= 0 && autoCleaning) {
            RemoveItem(item);
        }
    }

    private ItemDefinition GetItemDefinition(ItemID id)
    {
        if (itemsList == null) return null;

        return itemsList.GetItem(id);
    }

    private bool ShouldAddItem(ItemID id, int amount)
    {
        var item = GetInventoryItem(id);
        if (autoCleaning && ((item != null ? item.Amount : 0) + amount) <= 0) return false;

        return true;
    }
}