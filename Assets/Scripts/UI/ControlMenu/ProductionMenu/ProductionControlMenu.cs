using UnityEngine;
using UnityEngine.UI;

public class ProductionControlMenu : ControlMenu
{
    [SerializeField] private ProducedResourcePanel producedResourcePanelPrefab;
    private ProducedResourcePanel[] spawnedProducedResourcePanels;

    [SerializeField] private LayoutGroup layoutGroup;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onClickedContextProductionButton += OnContextProductionButtonClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onClickedContextProductionButton -= OnContextProductionButtonClicked;
    }

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
    }

    private void CreatePanels()
    {
        ProductionModule module = SelectManager.Instance.GetSelectedBuilding().GetComponent<ProductionModule>();
        ProducedItem[] resources = module.ProductionLevelData.producedResources;
        int length = resources.Length;
        spawnedProducedResourcePanels = new ProducedResourcePanel[length];

        for (int i = 0; i < length; i++) {
            ProducedItem resource = resources[i];
            ProducedResourcePanel spawned = Instantiate(producedResourcePanelPrefab, layoutGroup.transform);
            spawned.Init(module, resource, i);
            spawnedProducedResourcePanels[i] = spawned;

            if (i != module.currentProductingItemIndex) continue;

            spawned.Select();
        }
    }

    private void ClearPanels()
    {
        if (spawnedProducedResourcePanels == null) return;

        foreach (var panel in spawnedProducedResourcePanels) {
            Destroy(panel.gameObject);
        }
        spawnedProducedResourcePanels = null;
    }

    protected override void UpdateName()
    {
        LocalizationItem localizedName = SelectManager.Instance.GetSelectedBuilding().BuildingData.LocalizationItem;
        selectedNameText.SetLocalizationItem(localizedName);
    }

    private void OnContextProductionButtonClicked()
    {
        Open();
    }
}
