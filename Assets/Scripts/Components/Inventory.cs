using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Progress;

public class StorageItem
{
    public ItemInstance item { get; private set; } = null;
    public int maxAmount { get; private set; } = 0;

    public StorageItem(ItemInstance item, int maxAmount)
    {
        this.item = item;
        this.maxAmount = maxAmount;
    }

    public void AddAmount(int amount)
    {
        item.AddAmount(amount, maxAmount);
    }

    public void RemoveAmount(int amount)
    {
        item.RemoveAmount(amount);
    }

    public void AddMaxAmount(int value)
    {
        maxAmount += value;
    }

    public void RemoveMaxAmount(int value)
    {
        maxAmount += value;
        maxAmount = math.max(maxAmount, 0);
    }
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private float maxWeight = 0;
    public float MaxWeight => maxWeight;
    private float currentWeight = 0;
    public float CurrentWeight => currentWeight;
    public Dictionary<int, StorageItem> items { get; private set; } = new Dictionary<int, StorageItem>();

    public event Action<ItemInstance> onChangedItemAmount;
    public event Action<StorageItem> onChangedItemMaxAmount;

    // Add Item
    public void AddItem(int id, int amount = 0, int maxAmount = 0)
    {
        if (items.ContainsKey(id)) {
            AddItemMaxAmount(id, maxAmount);
            AddItemAmount(id, amount);
        }
        else {
            AddNewItem(id, amount, maxAmount);
        }
    }

    public void AddItemAmount(int id, int amount)
    {
        if (!HasItem(id)) {
            PrintHasNotItemError(id);
            return;
        }

        items[id].AddAmount(amount);

        ItemInstance item = items[id].item;
        OnChangeItemAmount(item);
        onChangedItemAmount?.Invoke(item);
    }

    public void AddItemMaxAmount(int id, int amount)
    {
        if (!HasItem(id)) {
            PrintHasNotItemError(id);
            return;
        }

        items[id].AddMaxAmount(amount);
        onChangedItemMaxAmount?.Invoke(items[id]);
    }

    private void AddNewItem(int id, int amount = 0, int maxAmount = 0)
    {
        if (HasItem(id)) {
            PrintHasItemError(id);
            return;
        }

        ItemData data = ItemsList.Instance.Items[id];
        ItemInstance item = new ItemInstance(data, amount);
        StorageItem storageItem = new StorageItem(item, maxAmount);
        items.Add(id, storageItem);
    }

    // Remove Item
    public void RemoveItem(int id)
    {
        if (!HasItem(id)) {
            PrintHasNotItemError(id);
            return;
        }

        items.Remove(id);
    }

    public void RemoveItemAmount(int id, int amount)
    {
        if (!HasItem(id)) {
            PrintHasNotItemError(id);
            return;
        }

        items[id].RemoveAmount(amount);

        ItemInstance item = items[id].item;
        OnChangeItemAmount(item);
        onChangedItemAmount?.Invoke(item);
    }

    public void RemoveItemMaxAmount(int id, int amount)
    {
        if (!HasItem(id)) {
            PrintHasNotItemError(id);
            return;
        }

        items[id].RemoveMaxAmount(amount);
        onChangedItemMaxAmount?.Invoke(items[id]);
    }

    // On Change Item Amount
    private void OnChangeItemAmount(ItemInstance item)
    {
        ChangeCurrentWeight(item);
    }

    private void ChangeCurrentWeight(ItemInstance item)
    {
        int weight = item.ItemData.Weight;
        int amount = item.Amount;
        currentWeight = weight * amount;
    }

    // Checks
    private bool HasItem(int id)
    {
        return items.ContainsKey(id);
    }

    private void PrintHasItemError(int id)
    {
        Debug.LogError($"Inventory is already has item by id {id}.");
    }

    private void PrintHasNotItemError(int id)
    {
        Debug.LogError($"Inventory has not an item by id {id}.");
    }

    //// Items
    //private void TakeItem(int itemId, int itemAmount)
    //{
    //    TakeItem_Internal(itemId, itemAmount);
    //}

    //private void TakeItem(ItemInstance item)
    //{
    //    TakeItem_Internal(item.ItemData.ItemId, item.Amount);
    //}

    //private void TakeItem_Internal(int itemId, int itemAmount)
    //{
    //    if (!carriedItemsDict.ContainsKey(itemId))
    //    {
    //        ItemInstance item = new ItemInstance(ItemsList.Instance.Items[itemId]); // The same item instance for list and dictionary.
    //        carriedItems.Add(item);
    //        carriedItemsDict.Add(itemId, item);
    //    }

    //    // We can change only the list or dictionary because we use the same item instance for them.
    //    carriedItemsDict[itemId].AddAmount(itemAmount);
    //}

    //private int SpendItem(int itemId, int amount)
    //{
    //    return SpendItem_Internal(itemId, carriedItemsDict[itemId].SubtractAmount(amount));
    //}

    //private int SpendItem(ItemInstance item)
    //{
    //    int id = item.ItemData.ItemId;
    //    int amount = item.Amount;
    //    return SpendItem_Internal(id, carriedItemsDict[id].SubtractAmount(amount));
    //}

    //private int SpendItem_Internal(int itemId, int amount)
    //{
    //    return carriedItemsDict[itemId].SubtractAmount(amount);
    //}

    //private void DeliverItem(Building building, ItemInstance item)
    //{
    //    DeliverItem_Internal(building, item);
    //}

    //private void DeliverItems(Building building, List<ItemInstance> items)
    //{
    //    for (int i = 0; i < items.Count; i++)
    //        DeliverItem_Internal(building, items[i]);
    //}

    //private void DeliverItem_Internal(Building building, ItemInstance item)
    //{
    //    StorageBuildingModule storage = building.GetComponent<StorageBuildingModule>();
    //    if (storage.storedItems.ContainsKey(item.ItemData.ItemId)) {
    //        int id = item.ItemData.ItemId;
    //        int amountToSpend = storage.AddItem(item);
    //        SpendItem(id, amountToSpend);
    //        //building.storageComponent.AddItem(item.ItemData.ItemId, SpendItem(item));
    //    }
    //}
}
