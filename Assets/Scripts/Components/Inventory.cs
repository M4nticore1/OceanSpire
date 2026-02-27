using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StorageItem
{
    public ItemInstance item { get; private set; } = null;
    public int maxAmount { get; private set; } = 0;

    public StorageItem(ItemInstance item)
    {
        this.item = item;
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
        maxAmount -= value;
        maxAmount = math.max(0, maxAmount);
    }
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private bool autoCleaning = false;
    [SerializeField] private float maxWeight = 0;
    public float MaxWeight => maxWeight;
    private float currentWeight = 0;
    public float CurrentWeight => currentWeight;
    public float RemainingWeight => MaxWeight - CurrentWeight;
    public List<StorageItem> items { get; private set; } = new List<StorageItem>();
    public Dictionary<int, StorageItem> itemsDict { get; private set; } = new Dictionary<int, StorageItem>();

    public event Action<ItemInstance> onChangedItemAmount;
    public event Action<StorageItem> onChangedItemMaxAmount;

    // Add Item
    public void AddItem(int id, int amount = 0, int maxAmount = 0)
    {
        TryAddNewItem(id);

        if (maxAmount > 0) {
            AddItemMaxAmount(id, maxAmount);
        }

        if (amount > 0) {
            AddItemAmount(id, amount);
        }
    }

    private void TryAddNewItem(int id)
    {
        if (itemsDict.ContainsKey(id)) return;

        AddNewItem(id);
    }

    private void AddItemAmount(int id, int amount)
    {
        itemsDict[id].AddAmount(amount);

        AddWeigth(id, amount);

        ItemInstance item = itemsDict[id].item;
        onChangedItemAmount?.Invoke(item);
    }

    private void AddItemMaxAmount(int id, int amount)
    {
        itemsDict[id].AddMaxAmount(amount);
        onChangedItemMaxAmount?.Invoke(itemsDict[id]);
    }

    private void AddNewItem(int id)
    {
        ItemData data = ItemsList.Instance.Items[id];
        ItemInstance item = new ItemInstance(data);
        StorageItem storageItem = new StorageItem(item);
        items.Add(storageItem);
        itemsDict.Add(id, storageItem);
    }

    private void RemoveItem(int id)
    {
        itemsDict.Remove(id);

        for (int i = 0; i < items.Count; i++) {
            StorageItem item = items[i];

            if (item.item.ItemData.ItemId == id) {
                items.RemoveAt(i);
            }
        }
    }

    public void RemoveItemAmount(int id, int amount)
    {
        if (!itemsDict.ContainsKey(id)) {
            PrintHasNotItemError(id);
            return;
        }

        itemsDict[id].RemoveAmount(amount);

        ItemInstance item = itemsDict[id].item;

        if (autoCleaning && item.Amount == 0) {
            RemoveItem(id);
        }

        RemoveWeigth(id, amount);

        onChangedItemAmount?.Invoke(item);
    }

    public void RemoveItemMaxAmount(int id, int amount)
    {
        if (!itemsDict.ContainsKey(id)) {
            PrintHasNotItemError(id);
            return;
        }

        itemsDict[id].RemoveMaxAmount(amount);

        // Remove Amount
        if (itemsDict[id].maxAmount < itemsDict[id].item.Amount) {
            int amountToRemove = itemsDict[id].item.Amount - itemsDict[id].maxAmount;
            RemoveItemAmount(id, amountToRemove);
        }

        onChangedItemMaxAmount?.Invoke(itemsDict[id]);
    }

    // On Change Item Amount
    private void OnChangeItemAmount(ItemInstance item)
    {
        ChangeCurrentWeight(item);
    }

    private void ChangeCurrentWeight(ItemInstance item)
    {
        float weight = item.ItemData.Weight;
        int amount = item.Amount;
        currentWeight = weight * amount;
    }

    private void AddWeigth(int id, int amount)
    {
        currentWeight += itemsDict[id].item.ItemData.Weight * amount;
    }

    private void RemoveWeigth(int id, int amount)
    {
        currentWeight -= itemsDict[id].item.ItemData.Weight * amount;
    }

    private void PrintHasItemError(int id)
    {
        Debug.LogError($"Inventory is already has item by id {id}.");
    }

    private void PrintHasNotItemError(int id)
    {
        Debug.LogError($"Inventory has no an item by id {id}.");
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
