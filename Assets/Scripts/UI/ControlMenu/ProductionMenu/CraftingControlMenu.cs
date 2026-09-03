using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingControlMenu : ControlMenu
{
    [Header("Crafting Menu")]
    [SerializeField] private CraftItemPanel producedResourcePanelPrefab;
    private List<CraftItemPanel> spawnedCraftResourcePanels = new();

    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    private Building building;

    protected override void UpdateMenu()
    {
        DestroyCraftWidgets();
        CreateCraftWidgets();
        FitLayoutGroupSize();
    }

    protected override ILocalizable GetTargetNameText()
    {
        return building;
    }

    protected override ILocalizable GetTargetDescriptionText()
    {
        return building;
    }

    public void Show(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] Building is not valid!");
            return;
        }

        this.building = building;
        Show();
    }

    private void CreateCraftWidgets()
    {
        var selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
        if (selectedBuilding == null) {
            Debug.LogError("SelectedBuilding is not valid");
            return;
        }

        var module = selectedBuilding.GetComponent<CraftingModule>();
        if (module == null) {
            Debug.LogError($"{selectedBuilding} does not have a CraftingModule");
            return;
        }

        var craftingLevelData = module.ProductionLevelData;
        if (craftingLevelData == null) {
            Debug.LogError($"{module} doesn not have a LevelData");
            return;
        }

        var craftItems = module.CraftItems;
        var craftDefinitions = craftingLevelData.CraftItems;

        for (int i = 0; i < craftDefinitions.Length; i++) {
            var craftItem = craftItems[i];
            var craftDefinition = craftDefinitions[i];

            var spawnedPanel = Instantiate(producedResourcePanelPrefab, layoutGroup.transform);
            spawnedPanel.Init(module, craftItem, selectGroup);

            spawnedCraftResourcePanels.Add(spawnedPanel);

            if (i != module.GetIndexOfCurrentCraftItem()) continue;

            spawnedPanel.Select();
        }
    }

    private void DestroyCraftWidgets()
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
        fitSizeToChildren.UpdateSizeDelay();
    }
}