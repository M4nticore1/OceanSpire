using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private bool autoCleaning = false;
    [SerializeField] private bool isUnlimitedAmount = false;
    public bool IsUnlimitedAmount => isUnlimitedAmount;

    [SerializeField] private bool isUnlimitedWeight = false;
    public bool IsUnlimitedWeight => isUnlimitedWeight;

    [SerializeField] private float maxWeight = 0;
    public float MaxWeight => maxWeight;

    private float currentWeight = 0;
    public float CurrentWeight => currentWeight;

    public float RemainingWeight => MaxWeight - CurrentWeight;

    public List<StorageItem> Items { get; private set; } = new();
    private Dictionary<int, StorageItem> itemsDictId = new();

    public event Action<ItemInstance> onAddedItemAmount;
    public event Action<ItemInstance> onRemovedItemAmount;

    public event Action<StorageItem> onAddedMaxItemAmount;
    public event Action<StorageItem> onRemovedMaxItemAmount;

    public event Action<ItemInstance> onChangedItemAmount;
    public event Action<StorageItem> onChangedItemMaxAmount;

    // Add Item
    public void AddItemAmount(int id, int amount)
    {
        TryAddNewItem(id);

        if (isUnlimitedAmount) {
            AddItemMaxAmount(id, amount);
        }

        if (!isUnlimitedWeight) {
            ItemData data = ItemsList.Instance.Items[id];
            amount = math.clamp(amount, 0, (int)(RemainingWeight / data.Weight));
        }

        itemsDictId[id].AddAmount(amount);
        AddWeigth(id, amount);

        ItemInstance item = itemsDictId[id].item;

        onAddedItemAmount?.Invoke(item);
        onChangedItemAmount?.Invoke(item);
    }

    public void AddItemMaxAmount(int id, int amount)
    {
        TryAddNewItem(id);

        itemsDictId[id].AddMaxAmount(amount);

        onAddedMaxItemAmount?.Invoke(itemsDictId[id]);
        onChangedItemMaxAmount?.Invoke(itemsDictId[id]);
    }

    public void TryAddNewItem(int id)
    {
        if (itemsDictId.ContainsKey(id))
            return;

        AddNewItem(id);
    }

    private void AddNewItem(int id)
    {
        ItemData data = ItemsList.Instance.GetItem(id);
        ItemInstance item = new ItemInstance(data);
        StorageItem storageItem = new StorageItem(item);

        Items.Add(storageItem);
        itemsDictId.Add(id, storageItem);
    }

    private void RemoveItem(int id)
    {
        itemsDictId.Remove(id);

        for (int i = 0; i < Items.Count; i++) {
            StorageItem item = Items[i];

            if (item.item.ItemData.ItemId == id) {
                Items.RemoveAt(i);
            }
        }
    }

    public void RemoveItemAmount(int id, int amount)
    {
        if (!itemsDictId.ContainsKey(id)) {
            PrintHasNotItemError(id);
            return;
        }

        itemsDictId[id].RemoveAmount(amount);

        ItemInstance item = itemsDictId[id].item;
        RemoveWeigth(id, amount);

        if (autoCleaning && item.Amount == 0) {
            RemoveItem(id);
        }

        onRemovedItemAmount?.Invoke(item);
        onChangedItemAmount?.Invoke(item);
    }

    public void RemoveItemMaxAmount(int id, int amount)
    {
        if (!itemsDictId.ContainsKey(id)) {
            PrintHasNotItemError(id);
            return;
        }

        itemsDictId[id].RemoveMaxAmount(amount);

        // Remove Amount
        if (itemsDictId[id].maxAmount < itemsDictId[id].item.Amount) {
            int amountToRemove = itemsDictId[id].item.Amount - itemsDictId[id].maxAmount;
            RemoveItemAmount(id, amountToRemove);
        }

        onRemovedMaxItemAmount?.Invoke(itemsDictId[id]);
        onChangedItemMaxAmount?.Invoke(itemsDictId[id]);
    }

    public StorageItem GetItem(int id)
    {
        StorageItem item;
        itemsDictId.TryGetValue(id, out item);

        return item;
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
        currentWeight += itemsDictId[id].item.ItemData.Weight * amount;
    }

    private void RemoveWeigth(int id, int amount)
    {
        currentWeight -= itemsDictId[id].item.ItemData.Weight * amount;
    }

    private void PrintHasItemError(int id)
    {
        Debug.LogError($"Inventory is already has item by id {id}.");
    }

    private void PrintHasNotItemError(int id)
    {
        Debug.LogError($"Inventory has no an item by id {id}.");
    }
}
