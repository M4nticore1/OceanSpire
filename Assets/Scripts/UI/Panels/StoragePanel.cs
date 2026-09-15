using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoragePanel : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ItemWidget itemWidgetPrefab;

    [Header("UI")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private FitSizeToContent fitSize;

    private List<ItemWidget> spawnedWidgets = new();

    private void Awake()
    {
        if (itemWidgetPrefab == null) {
            Debug.LogError($"[{nameof(StoragePanel)}] Item Widget Prefab is not valid!");
        }
    }

    public void SetItemsAndApply(IReadOnlyList<ItemInstance> items)
    {
        if (items == null) return;

        DestroyWidgets();
        CreateWidgets(items);
        UpdateSize();
    }

    private void CreateWidgets(IReadOnlyList<ItemInstance> items)
    {
        if (items == null) {
            Debug.LogError($"[{nameof(StoragePanel)}] Items is not valid!");
            return;
        }

        if (itemWidgetPrefab == null) return;

        foreach (var item in items) {
            if (item == null) continue;

            CreateWidget(item);
        }
    }

    private void CreateWidget(ItemInstance item)
    {
        if (item == null) return;

        var widget = Instantiate(itemWidgetPrefab, layoutGroup.transform);
        widget.AddAmount(item);

        spawnedWidgets.Add(widget);
    }

    private void DestroyWidgets()
    {
        if (spawnedWidgets == null) return;

        for (int i = spawnedWidgets.Count - 1; i >= 0; --i) {
            var widget = spawnedWidgets[i];
            if (widget == null) continue;

            Destroy(widget.gameObject);
            spawnedWidgets.RemoveAt(i);
        }
    }

    private void UpdateSize()
    {
        if (fitSize == null) return;

        fitSize.UpdateSize();
    }
}