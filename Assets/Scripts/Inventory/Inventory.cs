using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour, IContextable, ILocalizable
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
    private Dictionary<ItemStackId, ItemStackInstance> itemStacksDict = new();

    private ItemsList itemsList => ItemsList.Instance;

    [Header("Context Menu")]
    [SerializeField] public bool ignoreContextMenu = false;
    public bool IgnoreContextMenu
    {
        get {
            return ignoreContextMenu;
        }
        set {
            ignoreContextMenu = value;
        }
    }

    public event Action<ItemInstance> OnItemAdded;
    public event Action<ItemInstance> OnItemRemoved;

    public event Action<ItemInstance> OnItemAmountAdded;
    public event Action<ItemInstance> OnItemAmountRemoved;

    public event Action<StorageItem> OnItemLimitAdded;
    public event Action<StorageItem> OnItemLimitRemoved;

    public event Action<ItemInstance> OnItemAmountChanged;
    public event Action<StorageItem> OnItemLimitChanged;

    public event Action<ItemStackInstance> OnStackItemAmountChanged;
    public event Action<ItemStackInstance> OnStackLimitChanged;

    private void Awake()
    {
        InitStacks();
    }

    private void OnDestroy()
    {
        foreach (var item in items) {
            if (item == null) continue;

            UnsubscribeItem(item);
        }

        foreach (var stack in itemStacksDict.Values) {
            if (stack == null) continue;

            UnsubscribeStack(stack);
        }
    }

    public void Init()
    {
        Init(InventoryData.Default() ?? new InventoryData());
    }

    public void Init(InventoryData inventoryData)
    {
        if (inventoryData == null) {
            Debug.LogError($"[{nameof(Inventory)}] Inventory Data is not valid!");
            Init();
            return;
        }

        var itemsData = inventoryData.Items;

        if (itemsData != null) {
            foreach (var itemData in itemsData) {
                if (itemData == null) continue;

                AddItemAmount(itemData.Id, itemData.Amount);
            }
        }
    }

    // --- Add Item ---
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
            item.SetStack(GetStack(item.Definition.StackDefinition.StackId));
        }

        items.Add(item);
        itemsDict.Add(item.Definition.ItemId, item);

        OnItemAdded?.Invoke(item);
    }

    // --- Add Item Amount ---
    public void AddItemAmountRange(IReadOnlyList<ItemInstance> items)
    {
        if (items == null) return;

        foreach (var item in items) {
            if (item == null) continue;

            AddItemAmount(item);
        }
    }

    public void AddItemAmount(ItemInstance item)
    {
        AddItemAmount(item.Definition.ItemId, item.Amount);
    }

    public void AddItemAmount(ItemID id, int amount)
    {
        var definition = GetItemDefinition(id);
        if (definition == null)
            return;

        if (useWeightLimit && definition.Weight > 0f) {
            amount = Mathf.Min(amount, Mathf.FloorToInt(GetRemainingWeight() / definition.Weight));
        }

        if (!ShouldAddItem(id, amount))
            return;

        var item = GetInventoryItem(id);
        if (item == null) {
            item = AddItem(id);

            if (item == null)
                return;
        }

        item.AddAmount(amount);
    }

    // --- Remove Item ---
    public void RemoveItem(ItemInstance item)
    {
        if (item == null)
            return;

        UnsubscribeItem(item);

        if (item.Stack != null) {
            item.Stack.RemoveItem(item);
        }

        items.Remove(item);
        itemsDict.Remove(item.Definition.ItemId);

        OnItemRemoved?.Invoke(item);
    }

    public void Clear()
    {
        for (int i = items.Count - 1; i >= 0; i--) {
            var item = items[i];
            if (item == null) continue;

            RemoveItem(item);
        }
    }

    // --- Remove Item Amount ---
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

    // --- Add Limit ---
    public void AddLimit(ItemStackId stack, int amount)
    {
        GetStack(stack).AddLimit(amount);
    }

    // --- Remove Limit ---
    public void RemoveLimit(ItemStackId stack, int amount)
    {
        GetStack(stack).RemoveLimit(amount);
    }

    // --- Get Item ---
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

    // --- Get Stack ---
    public int GetLimit(ItemStackId stack)
    {
        return GetStack(stack).Amount;
    }

    public ItemStackInstance GetStack(ItemStackId stack)
    {
        if (!itemStacksDict.TryGetValue(stack, out var itemStack)) {
            Debug.LogError($"[{nameof(Inventory)}] Stack ({stack}) is not valid!");
        }

        return itemStack;
    }

    // --- Weight ---
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

    // --- ILocalizable ---
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "weight", Mathf.RoundToInt(GetCurrentWeight()).ToString("F0") },
            { "maxWeight", Mathf.RoundToInt(weightLimit).ToString("F0") }
        };
    }

    // --- Add Stack ---
    private void InitStacks()
    {
        var stackDefinitions = ItemStacksList.Instance.StackDefinitions;

        foreach (var definition in stackDefinitions) {
            if (definition == null) continue;

            AddStack(new ItemStackInstance(definition));
        }
    }

    private void AddStack(ItemStackInstance stack)
    {
        if (stack == null) {
            Debug.LogError($"[{nameof(Inventory)}] Stack is not valid!");
            return;
        }

        itemStacksDict.Add(stack.Definition.StackId, stack);
        SubscribeStack(stack);
    }

    // --- Item Subscribe ---
    private void SubscribeItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(Inventory)}] Item is not valid!");
            return;
        }

        item.OnItemAmountAdded += HandleItemAmountAdded;
        item.OnItemAmountRemoved += HandleItemAmountRemoved;
    }

    private void UnsubscribeItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(Inventory)}] Item is not valid!");
            return;
        }

        item.OnItemAmountAdded -= HandleItemAmountAdded;
        item.OnItemAmountRemoved -= HandleItemAmountRemoved;
    }

    // --- Stack Subscribe ---
    private void SubscribeStack(ItemStackInstance stack)
    {
        if (stack == null) {
            Debug.LogError($"[{nameof(Inventory)}] Stack is not valid!");
            return;
        }

        stack.OnItemAmountChanged += HandleStackItemAmounChanged;
    }

    private void UnsubscribeStack(ItemStackInstance stack)
    {
        if (stack == null) {
            Debug.LogError($"[{nameof(Inventory)}] Stack is not valid!");
            return;
        }

        stack.OnItemAmountChanged -= HandleStackItemAmounChanged;
    }

    // --- Item Amount Events ---
    private void HandleItemAmountAdded(ItemInstance item, int amount)
    {
        var stack = GetStack(item.Definition.StackDefinition.StackId);
        stack.AddItem(item);

        OnItemAmountAdded?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);
    }

    private void HandleItemAmountRemoved(ItemInstance item, int amount)
    {
        if (item == null) return;

        if (item.Amount <= 0 && autoCleaning) {
            RemoveItem(item);
        }

        OnItemAmountRemoved?.Invoke(item);
        OnItemAmountChanged?.Invoke(item);
    }

    // --- Stack Amount Events ---
    private void HandleStackItemAmounChanged(ItemStackInstance stack)
    {
        if (stack == null) return;

        OnStackItemAmountChanged?.Invoke(stack);
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