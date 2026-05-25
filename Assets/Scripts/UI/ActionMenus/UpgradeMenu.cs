using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeMenu : UIBehaviour
{
    [SerializeField] private ResourceWidget resourceWidgetPrefab;

    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private CustomButton upgradeButton;
    [SerializeField] private CustomButton closeButton;
    [SerializeField] private GridLayoutGroup layoutGroup;

    [SerializeField] private TextLocalizer targetLocalizer;

    private UpgradeComponent upgradeComponent;
    private List<ResourceWidget> spawnedResourceWidgets = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        upgradeButton.OnReleased.AddListener(OnUpgradeButtonClicked);
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        upgradeButton.OnReleased.RemoveListener(OnUpgradeButtonClicked);
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    public void Open(Building building)
    {
        upgradeComponent = building.UpgradeComponent;
        slidePanel.Open();

        ClearWidgets();
        CreateWidgets(building);
        UpdateTargetName(building);
    }

    public void Close()
    {
        slidePanel.Close();
    }

    private void CreateWidgets(Building building)
    {
        foreach (var item in building.NextLevelData.ResourcesToBuild) {
            var widget = ResourceWidgetFactory.CreateResourceWidget(resourceWidgetPrefab, layoutGroup.transform);
            spawnedResourceWidgets.Add(widget);

            widget.SetAmount(cityStorage.Inventory.GetItemById(item.Definition.ItemId));
            widget.SetLimit(item);
        }
    }

    private void ClearWidgets()
    {
        for (int i = 0; i < spawnedResourceWidgets.Count; i++) {
            Destroy(spawnedResourceWidgets[i].gameObject);
            spawnedResourceWidgets.RemoveAt(i);
        }
    }

    private void UpdateTargetName(Building building)
    {
        targetLocalizer.SetPlaceHolderLocalization(building);
        targetLocalizer.UpdateText();
    }

    private void OnUpgradeButtonClicked()
    {
        upgradeComponent.Upgrade();
        Close();
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }
}