using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private ResourceWidget resourceWidgetPrefab;

    [Header("UI")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;
    [SerializeField] private TextLocalizer weightText;
    [SerializeField] private GameObject emptyText;

    private Inventory inventory;
    private Dictionary<ItemDefinition, ResourceWidget> spawnedResourceWidgets = new();

    private bool isSubscribed = false;

    private void TrySubscribe(Inventory inventory)
    {
        if (isSubscribed) return;
        if (!inventory) return;

        inventory.OnItemAdded += OnItemAdded;
        inventory.OnItemRemoved += OnItemRemoved;
        inventory.OnItemAmountChanged += OnItemAmountChanged;

        isSubscribed = true;
    }

    private void TryUnsubscribe(Inventory inventory)
    {
        if (!isSubscribed) return;
        if (!inventory) return;

        inventory.OnItemAdded -= OnItemAdded;
        inventory.OnItemRemoved -= OnItemRemoved;
        inventory.OnItemAmountChanged -= OnItemAmountChanged;

        isSubscribed = false;
    }

    public void SetInventoryAndApply(Inventory inventory)
    {
        if (!inventory) {
            Debug.LogError($"[{nameof(InventoryPanel)}] Inventory is not valid!");
            return;
        }

        TryUnsubscribe(inventory);

        this.inventory = inventory;

        RemoveWidgets();
        CreateWidgets();

        UpdateLayoutGroupSize();
        UpdateWeightText();
        UpdateEmptyTextActive();
        ResetScroll();

        TrySubscribe(inventory);
    }

    private void CreateWidgets()
    {
        foreach (var item in inventory.Items) {
            if (item == null) continue;
            if (item.Amount <= 0) continue;

            CreateWidget(item);
        }
    }

    private void RemoveWidgets()
    {
        foreach (var widget in spawnedResourceWidgets.Values) {
            if (!widget) {
                Debug.LogError($"[{nameof(InventoryPanel)}] Spawned Widget is not valid!");
                continue;
            }

            Destroy(widget.gameObject);
        }

        spawnedResourceWidgets.Clear();
    }

    private void CreateWidget(ItemInstance item)
    {
        if (item == null) return;
        if (item.Amount <= 0) return;

        var widget = Instantiate(resourceWidgetPrefab, layoutGroup.transform);
        widget.SetItem(item.Definition);
        widget.AddAmount(item);

        spawnedResourceWidgets.Add(item.Definition, widget);
    }

    private void RemoveWidget(ItemInstance item)
    {
        if (item == null) return;

        var widget = spawnedResourceWidgets[item.Definition];
        Destroy(widget.gameObject);
        spawnedResourceWidgets.Remove(item.Definition);
    }

    private void UpdateWeightText()
    {
        if (!weightText) return;

        weightText.SetPlaceHolderLocalization(inventory);
    }

    private void UpdateLayoutGroupSize()
    {
        fitSizeToChildren.UpdateSize();
    }

    private void UpdateEmptyTextActive()
    {
        if (!emptyText) return;

        emptyText.SetActive(spawnedResourceWidgets.Count <= 0);
    }

    private void ResetScroll()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnItemAdded(ItemInstance item)
    {
        CreateWidget(item);
        UpdateLayoutGroupSize();
        UpdateWeightText();
        UpdateEmptyTextActive();
    }

    private void OnItemRemoved(ItemInstance item)
    {
        RemoveWidget(item);
        UpdateLayoutGroupSize();
        UpdateWeightText();
        UpdateEmptyTextActive();
    }

    private void OnItemAmountChanged(ItemInstance item)
    {
        UpdateWeightText();
        UpdateEmptyTextActive();
    }
}