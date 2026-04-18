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

    public List<StorageItem> items { get; private set; } = new List<StorageItem>();
    public Dictionary<int, StorageItem> itemsDict { get; private set; } = new Dictionary<int, StorageItem>();

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

        itemsDict[id].AddAmount(amount);
        AddWeigth(id, amount);

        ItemInstance item = itemsDict[id].item;
        onChangedItemAmount?.Invoke(item);
    }

    public void AddItemMaxAmount(int id, int amount)
    {
        TryAddNewItem(id);

        itemsDict[id].AddMaxAmount(amount);
        onChangedItemMaxAmount?.Invoke(itemsDict[id]);
    }

    public void TryAddNewItem(int id)
    {
        if (itemsDict.ContainsKey(id))
            return;

        AddNewItem(id);
    }

    private void AddNewItem(int id)
    {
        ItemData data = ItemsList.Instance.GetItemData(id);
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
        RemoveWeigth(id, amount);

        if (autoCleaning && item.Amount == 0) {
            RemoveItem(id);
        }

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
}
