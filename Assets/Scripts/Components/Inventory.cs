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

    private List<ItemInstance> items = new();
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
        ItemStackEnum[] stackValues = (ItemStackEnum[])Enum.GetValues(typeof(ItemStackEnum));

        foreach (var stackEnum in stackValues) {
            itemStacks.Add(stackEnum, new ItemStack(stackEnum));
        }
    }

    public void AddItem(int id, int amount)
    {
        ItemInstance item = GetItemById(id);

        if (item == null) {
            item = AddItem(id);
        }

        ItemStack stack = GetStack(item.Definition.Stack);
        amount = math.clamp(amount, 0, stack.Amount - stack.TotalAmount);

        item.AddAmount(amount);
        stack.AddAmount(amount);
    }

    public void RemoveItem(int id, int amount)
    {
        ItemInstance item = GetItemById(id);
        amount = math.clamp(amount, 0, item.Amount);

        item.RemoveAmount(amount);

        if (item.Amount <= 0) {
            RemoveItem(id);
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

    public ItemInstance GetItemById(int id)
    {
        ItemInstance item;
        itemsDict.TryGetValue(id, out item);

        return item;
    }

    public ItemInstance GetItemByIndex(int index)
    {
        if (index >= items.Count) return null;

        return items[index];
    }

    private ItemInstance AddItem(int id)
    {
        ItemDefinition definition = ItemsList.Instance.GetItem(id);
        ItemInstance item = new ItemInstance(definition);

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

    //// Add Item
    //public void AddItemAmount(int id, int amount)
    //{
    //    TryAddNewItem(id);

    //    if (isUnlimitedAmount) {
    //        AddItemMaxAmount(id, amount);
    //    }

    //    if (!isUnlimitedWeight) {
    //        ItemDefinition data = ItemsList.Instance.Items[id];
    //        amount = math.clamp(amount, 0, (int)(RemainingWeight / data.Weight));
    //    }

    //    itemsDictId[id].AddAmount(amount);
    //    AddWeigth(id, amount);

    //    ItemInstance item = itemsDictId[id].item;

    //    onAddedItemAmount?.Invoke(item);
    //    onChangedItemAmount?.Invoke(item);
    //}

    //public void AddItemMaxAmount(int id, int amount)
    //{
    //    TryAddNewItem(id);

    //    itemsDictId[id].AddMaxAmount(amount);

    //    onAddedMaxItemAmount?.Invoke(itemsDictId[id]);
    //    onChangedItemMaxAmount?.Invoke(itemsDictId[id]);
    //}

    //public void TryAddNewItem(int id)
    //{
    //    if (itemsDictId.ContainsKey(id))
    //        return;

    //    AddNewItem(id);
    //}

    //private void AddNewItem(int id)
    //{
    //    ItemDefinition data = ItemsList.Instance.GetItem(id);
    //    ItemInstance item = new ItemInstance(data);
    //    StorageItem storageItem = new StorageItem(item);

    //    StorageItems.Add(storageItem);
    //    itemsDictId.Add(id, storageItem);
    //}

    //private void RemoveItem(int id)
    //{
    //    itemsDictId.Remove(id);

    //    for (int i = 0; i < StorageItems.Count; i++) {
    //        StorageItem item = StorageItems[i];

    //        if (item.item.ItemData.ItemId == id) {
    //            StorageItems.RemoveAt(i);
    //        }
    //    }
    //}

    //public void RemoveItemAmount(int id, int amount)
    //{
    //    if (!itemsDictId.ContainsKey(id)) {
    //        PrintHasNotItemError(id);
    //        return;
    //    }

    //    itemsDictId[id].RemoveAmount(amount);

    //    ItemInstance item = itemsDictId[id].item;
    //    RemoveWeigth(id, amount);

    //    if (autoCleaning && item.Amount == 0) {
    //        RemoveItem(id);
    //    }

    //    onRemovedItemAmount?.Invoke(item);
    //    onChangedItemAmount?.Invoke(item);
    //}

    //public void RemoveItemMaxAmount(int id, int amount)
    //{
    //    if (!itemsDictId.ContainsKey(id)) {
    //        PrintHasNotItemError(id);
    //        return;
    //    }

    //    itemsDictId[id].RemoveMaxAmount(amount);

    //    // Remove Amount
    //    if (itemsDictId[id].maxAmount < itemsDictId[id].item.Amount) {
    //        int amountToRemove = itemsDictId[id].item.Amount - itemsDictId[id].maxAmount;
    //        RemoveItemAmount(id, amountToRemove);
    //    }

    //    onRemovedMaxItemAmount?.Invoke(itemsDictId[id]);
    //    onChangedItemMaxAmount?.Invoke(itemsDictId[id]);
    //}

    //public StorageItem GetItem(int id)
    //{
    //    StorageItem item;
    //    itemsDictId.TryGetValue(id, out item);

    //    return item;
    //}

    //// On Change Item Amount
    //private void OnChangeItemAmount(ItemInstance item)
    //{
    //    ChangeCurrentWeight(item);
    //}

    //private void ChangeCurrentWeight(ItemInstance item)
    //{
    //    float weight = item.ItemData.Weight;
    //    int amount = item.Amount;
    //    currentWeight = weight * amount;
    //}

    //private void AddWeigth(int id, int amount)
    //{
    //    currentWeight += itemsDictId[id].item.ItemData.Weight * amount;
    //}

    //private void RemoveWeigth(int id, int amount)
    //{
    //    currentWeight -= itemsDictId[id].item.ItemData.Weight * amount;
    //}

    //private void PrintHasItemError(int id)
    //{
    //    Debug.LogError($"Inventory is already has item by id {id}.");
    //}

    //private void PrintHasNotItemError(int id)
    //{
    //    Debug.LogError($"Inventory has no an item by id {id}.");
    //}
}
