using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoragePanel : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ItemStackWidget itemStackWidgetPrefab;

    [Header("UI")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private FitSizeToContent fitSize;

    private List<ItemStackWidget> spawnedWidgets = new();

    private void Awake()
    {
        if (itemStackWidgetPrefab == null) {
            Debug.LogError($"[{nameof(StoragePanel)}] Item Widget Prefab is not valid!");
        }
    }

    public void SetStacksAndApply(IReadOnlyList<ItemStackInstance> stacks)
    {
        if (stacks == null)
            return;

        DestroyWidgets();
        CreateWidgets(stacks);
        UpdateSize();
    }

    private void CreateWidgets(IReadOnlyList<ItemStackInstance> stacks)
    {
        if (stacks == null) {
            Debug.LogError($"[{nameof(StoragePanel)}] Stacks is not valid!");
            return;
        }

        if (itemStackWidgetPrefab == null)
            return;

        foreach (var stack in stacks) {
            if (stack == null)
                continue;

            CreateWidget(stack);
        }
    }

    private void CreateWidget(ItemStackInstance stackInstance)
    {
        if (stackInstance == null) {
            Debug.LogError($"[{nameof(StoragePanel)}] Stack Instance is not valid!");
            return;
        }

        var widget = Instantiate(itemStackWidgetPrefab, layoutGroup.transform);
        widget.SetStackInstance(stackInstance);
        widget.AddAmount(stackInstance);

        spawnedWidgets.Add(widget);
    }

    private void DestroyWidgets()
    {
        if (spawnedWidgets == null)
            return;

        for (int i = spawnedWidgets.Count - 1; i >= 0; --i) {
            var widget = spawnedWidgets[i];
            if (widget == null)
                continue;

            Destroy(widget.gameObject);
            spawnedWidgets.RemoveAt(i);
        }
    }

    private void UpdateSize()
    {
        if (fitSize == null)
            return;

        fitSize.RunUpdateSizeEndOfFrame();
    }
}