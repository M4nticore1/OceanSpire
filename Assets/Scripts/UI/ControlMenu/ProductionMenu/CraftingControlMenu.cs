using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingControlMenu : ControlMenu
{
    [SerializeField] private CraftItemPanel producedResourcePanelPrefab;
    private List<CraftItemPanel> spawnedCraftResourcePanels = new();

    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    protected override void OnOpen()
    {

    }

    protected override void OnClose()
    {

    }

    protected override void UpdateMenu()
    {
        ClearPanels();
        CreatePanels();
        FitLayoutGroupSize();
    }

    private void CreatePanels()
    {
        var selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
        if (!selectedBuilding) {
            Debug.LogError("SelectedBuilding is not valid");
            return;
        }

        var module = selectedBuilding.GetComponent<CraftingModule>();
        if (!module) {
            Debug.LogError($"{selectedBuilding} does not have a CraftingModule");
            return;
        }

        var craftingLevelData = module.ProductionLevelData;
        if (!craftingLevelData) {
            Debug.LogError($"{module} doesn not have a LevelData");
            return;
        }

        var crafts = craftingLevelData.CraftItems;

        for (int i = 0; i < crafts.Length; i++) {
            var craft = crafts[i];

            var spawned = Instantiate(producedResourcePanelPrefab, layoutGroup.transform);
            spawned.Init(module, craft, i, selectGroup);

            spawnedCraftResourcePanels.Add(spawned);

            if (i != module.CurrentProductingItemIndex) continue;

            spawned.Select();
        }
    }

    private void ClearPanels()
    {
        for (int i = spawnedCraftResourcePanels.Count - 1; i >= 0; i--) {
            var panel = spawnedCraftResourcePanels[i];
            Destroy(panel.gameObject);
            spawnedCraftResourcePanels.RemoveAt(i);
        }
    }

    private void FitLayoutGroupSize()
    {
        StartCoroutine(FitLayoutGroupSizeCoroutine());
    }

    private IEnumerator FitLayoutGroupSizeCoroutine()
    {
        yield return new WaitForEndOfFrame();

        scrollRect.verticalNormalizedPosition = 1f;
        fitSizeToChildren.UpdateSize();
    }
}