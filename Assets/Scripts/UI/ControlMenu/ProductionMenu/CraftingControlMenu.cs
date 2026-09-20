using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CraftingControlMenu : ControlMenu
{
    public static CraftingControlMenu Instance { get; private set; }

    [Header("Crafting Menu")]
    [SerializeField] private CraftItemWidget producedResourcePanelPrefab;

    [SerializeField] private CraftsPanel craftsPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private FitSizeToChildren fitSizeToChildren;

    private Building building;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null) {
            Instance = this;
        }
        else {
            Debug.LogError($"[{nameof(CraftingControlMenu)}] There's another Crafting Control Menu on the scene!");
        }
    }

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
        var module = building.GetComponent<CraftingModule>();
        if (module == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] CraftingModule is not valid at {building}!");
            return;
        }

        var craftingLevelData = module.ProductionLevelData;
        if (craftingLevelData == null) {
            Debug.LogError($"[{nameof(EquipmentMenu)}] LevelData is not valid at {building}");
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