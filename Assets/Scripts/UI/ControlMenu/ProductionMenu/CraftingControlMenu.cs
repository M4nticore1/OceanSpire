using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CraftingControlMenu : ControlMenu
{
    [Header("Crafting Menu")]
    [SerializeField] private CraftItemWidget producedResourcePanelPrefab;

    [SerializeField] private CraftsPanel craftsPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    private Building building;

    protected override void UpdateMenu()
    {
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
            Debug.LogError($"[{nameof(EquipmentMenu)}] SelectedBuilding is not valid");
            return;
        }

        var module = selectedBuilding.GetComponent<CraftingModule>();
        if (module == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] {selectedBuilding} does not have a CraftingModule");
            return;
        }

        var craftingLevelData = module.ProductionLevelData;
        if (craftingLevelData == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] {module} doesn not have a LevelData");
            return;
        }

        var craftDefinitions = craftingLevelData.CraftItems;
        craftsPanel.SetCraftsAndApply(module.CraftItems, module, selectGroup);
    }

    private void FitLayoutGroupSize()
    {
        StartCoroutine(FitLayoutGroupSizeCoroutine());
    }

    private IEnumerator FitLayoutGroupSizeCoroutine()
    {
        yield return new WaitForEndOfFrame();

        scrollRect.verticalNormalizedPosition = 1f;
        fitSizeToChildren.RunUpdateSizeEndOfFrame();
    }
}