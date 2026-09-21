using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemStackInstance : IAmountable, IInformationable, ILocalizable
{
    [SerializeField] private ItemStackDefinition definition;
    public ItemStackDefinition Definition => definition;

    [SerializeField, FormerlySerializedAs("limit")] private int amount = 0;
    public int Amount => amount;

    [SerializeField] private List<ItemInstance> items = new();
    public IReadOnlyList<ItemInstance> Items => items;

    public bool IsOverflowed => GetItemsAmountSum() >= amount;

    public event Action<IAmountable> OnAmountChanged;
    public event Action<ItemStackInstance> OnItemAmountChanged;

    public ItemStackInstance(ItemStackDefinition definition)
    {
        this.definition = definition;
    }

    // -- Add/Remove Limit ---
    public void AddLimit(int value)
    {
        if (value <= 0)
            return;

        SetLimit(amount + value);
    }

    public void RemoveLimit(int value)
    {
        if (value <= 0)
            return;

        SetLimit(amount - value);
    }

    // --- Add/Remove Item ---
    public void AddItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(ItemStackInstance)}] Item is not valid!");
            return;
        }

        if (items.Contains(item))
            return;

        items.Add(item);
        SubscribeItem(item);
    }

    public void RemoveItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(ItemStackInstance)}] Item is not valid!");
            return;
        }

        items.Remove(item);
        UnubscribeItem(item);
    }

    // --- Get Items Sum ---
    public int GetItemsAmountSum()
    {
        int sum = 0;

        foreach (var item in items) {
            if (item == null)
                continue;

            sum += Mathf.Max(0, item.Amount);
        }

        return sum;
    }

    // --- IInformationable ---
    public LocalizationItem GetInformationName()
    {
        return definition.NameLocalizationItem;
    }

    public LocalizationItem GetInformationDescription()
    {
        return definition.DescriptionLocalizationItem;
    }

    public Sprite GetInformationIcon()
    {
        return definition.Icon;
    }

    // ILocalizable
    public Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "stackName", LocalizationManager.Instance.GetLocalizedText(GetInformationName()) },
            { "stackAmount", GetItemsAmountSum().ToString() },
            { "stackLimit", amount.ToString() },
        };
    }

    // --- Set Limit ---
    private void SetLimit(int value)
    {
        amount = Mathf.Max(0, value);

        OnAmountChanged?.Invoke(this);
    }

    // --- Item Amount Events ---
    private void SubscribeItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(ItemStackInstance)}] Item is not valid!");
            return;
        }

        item.OnItemAmountChanged += HandleItemAmountChanged;
    }

    private void UnubscribeItem(ItemInstance item)
    {
        if (item == null) {
            Debug.LogError($"[{nameof(ItemStackInstance)}] Item is not valid!");
            return;
        }

        item.OnItemAmountChanged -= HandleItemAmountChanged;
    }

    // --- Subscribe/Unsubscribe Item
    private void HandleItemAmountChanged(ItemInstance item)
    {
        OnItemAmountChanged?.Invoke(this);
    }
}