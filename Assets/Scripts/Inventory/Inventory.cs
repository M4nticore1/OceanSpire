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
        if (!ShouldAddItem(id, amount)) return;

        var item = GetItem(id) ?? CreateAndRegisterItem(id);
        var stack = GetStack(item.Definition.Stack);

        if (useAmountLimit) {
            amount = Mathf.Clamp(amount, 0, stack.Amount - stack.GetItemAmountsSum());
        }

        if (useWeightLimit && item.Definition.Weight > 0) {
            float remainingWeight = weightLimit - currentWeight;
            amount = Mathf.Clamp(amount, 0, (int)(remainingWeight / item.Definition.Weight));
        }

        item.AddAmount(amount);
    }

    public void RemoveItem(ItemInstance item)
    {
        RemoveItem(item.Definition.ItemId, item.Amount);
    }

    public void RemoveItem(ItemID id, int amount)
    {
        var item = GetItem(id);
        if (item == null) return;

        item.RemoveAmount(Mathf.Max(0, amount));
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
        if (index < 0 || index >= items.Count) return null;
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

    private void UnregisterItem(ItemInstance item)
    {
        UnsubscribeItem(item);

        items.Remove(item);
        itemsDict.Remove(item.Definition.ItemId);

        OnItemRemoved?.Invoke(item);
    }

    private void SubscribeItem(ItemInstance item)
    {
        item.OnItemAmountAdded += HandleItemAmountAdded;
        item.OnItemAmountRemoved += HandleItemAmountRemoved;
    }

    private void UnsubscribeItem(ItemInstance item)
    {
        item.OnItemAmountAdded -= HandleItemAmountAdded;
        item.OnItemAmountRemoved -= HandleItemAmountRemoved;
    }

    private ItemInstance CreateAndRegisterItem(ItemID id)
    {
        var definition = ItemsList.Instance.GetItem(id);
        var item = definition.CreateInstance();

        SubscribeItem(item);
        item.SetStack(GetStack(item.Definition.Stack));

        if (itemsDict.TryAdd(id, item)) {
            items.Add(item);
        }

        OnItemAdded?.Invoke(item);
        return item;
    }

    private void AddWeight(float weight)
    {
        SetWeight(currentWeight + weight);
    }

    private void RemoveWeight(float weight)
    {
        if (items.Count > 0) {
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

    private void HandleItemAmountAdded(ItemInstance item, int amount)
    {
        AddWeight(amount * item.Definition.Weight);

        var stack = GetStack(item.Definition.Stack);
        stack.AddItemAmount(item);

        OnItemAmountAdded?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);
    }

    private void HandleItemAmountRemoved(ItemInstance item, int amount)
    {
        RemoveWeight(amount * item.Definition.Weight);

        var stack = GetStack(item.Definition.Stack);
        stack.RemoveItemAmount(item);

        OnItemAmountRemoved?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);

        if (item.Amount <= 0 && autoCleaning) {
            UnregisterItem(item);
        }
    }

    private bool ShouldAddItem(ItemID id, int amount)
    {
        var item = GetItem(id);
        if (((item != null ? item.Amount : 0) + amount) <= 0 && autoCleaning) return false;

        return true;
    }
}