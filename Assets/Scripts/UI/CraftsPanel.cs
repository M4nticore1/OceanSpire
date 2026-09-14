using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftsPanel : MonoBehaviour
{
    [SerializeField] private CraftItemWidget CraftItemWidget;
    [SerializeField] private LayoutGroup layoutGroup;

    private List<CraftItemWidget> spawnedWidgets = new();

    private void Awake()
    {
        if (CraftItemWidget == null) {
            Debug.LogError($"[{nameof(CraftsPanel)}] Craft Item Widget is not valid!");
            return;
        }
        if (layoutGroup == null) {
            Debug.LogError($"[{nameof(CraftsPanel)}] Layout Group is not valid!");
            return;
        }
    }

    public void SetCraftsAndApply(IReadOnlyList<CraftItemDefinition> crafts)
    {
        DestroyWidgets();
        CreateWidgets(crafts);
    }

    public void SetCraftsAndApply(IReadOnlyList<CraftItemInstance> crafts)
    {
        DestroyWidgets();
        CreateWidgets(crafts);
    }

    public void SetCraftsAndApply(IReadOnlyList<CraftItemInstance> crafts, CraftingModule craftingModule, SelectGroup selectGroup)
    {
        DestroyWidgets();
        CreateWidgets(crafts, craftingModule, selectGroup);
    }

    private void CreateWidgets(IReadOnlyList<CraftItemDefinition> crafts)
    {
        if (crafts == null) return;
        if (CraftItemWidget == null) return;
        if (layoutGroup == null) return;

        for (int i = 0; i < crafts.Count; i++) {
            var craftItem = crafts[i];

            var spawnedPanel = Instantiate(CraftItemWidget, layoutGroup.transform);
            spawnedPanel.Init(craftItem);

            spawnedWidgets.Add(spawnedPanel);
        }
    }

    private void CreateWidgets(IReadOnlyList<CraftItemInstance> crafts)
    {
        if (crafts == null) return;
        if (CraftItemWidget == null) return;
        if (layoutGroup == null) return;

        for (int i = 0; i < crafts.Count; i++) {
            var craftItem = crafts[i];

            var spawnedPanel = Instantiate(CraftItemWidget, layoutGroup.transform);
            spawnedPanel.Init(craftItem);

            spawnedWidgets.Add(spawnedPanel);
        }
    }

    private void CreateWidgets(IReadOnlyList<CraftItemInstance> crafts, CraftingModule craftingModule, SelectGroup selectGroup)
    {
        if (crafts == null) return;
        if (CraftItemWidget == null) return;
        if (layoutGroup == null) return;

        for (int i = 0; i < crafts.Count; i++) {
            var craftItem = crafts[i];

            var spawnedPanel = Instantiate(CraftItemWidget, layoutGroup.transform);
            spawnedPanel.Init(craftItem, craftingModule, selectGroup);

            spawnedWidgets.Add(spawnedPanel);

            if (i == craftingModule.GetIndexOfCurrentCraftItem()) {
                spawnedPanel.Select();
            }
        }
    }

    private void DestroyWidgets()
    {
        for (int i = spawnedWidgets.Count - 1; i >= 0; i--) {
            var panel = spawnedWidgets[i];
            if (panel == null) continue;

            Destroy(panel.gameObject);
            spawnedWidgets.RemoveAt(i);
        }
    }
}