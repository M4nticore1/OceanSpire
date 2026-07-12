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

            CreateWidget(item);
        }
    }

    private void RemoveWidgets()
    {
        foreach (var widget in spawnedResourceWidgets.Values) {
            Destroy(widget.gameObject);
        }

        spawnedResourceWidgets.Clear();
    }

    private void CreateWidget(ItemInstance item)
    {
        var widget = Instantiate(resourceWidgetPrefab, layoutGroup.transform);
        widget.SetItem(item.Definition);
        widget.AddAmount(item);

        spawnedResourceWidgets.Add(item.Definition, widget);
    }

    private void RemoveWidget(ItemInstance item)
    {
        Destroy(spawnedResourceWidgets[item.Definition].gameObject);
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