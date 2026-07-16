using UnityEngine;

public class UpgradeBuildingMenu : BuildingMenu
{
    [Header("Upgrade Menu")]
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private TextLocalizer buildTimeText;
    [SerializeField] private LocalizationItem buildTimeLocalization;
    [SerializeField] private LocalizationItem instantlyBuildLocalization;

    private UpgradeComponent upgradeComponent;

    protected override void OnOpened(Building building)
    {
        upgradeComponent = building.UpgradeComponent;
        UpdateBuildTimeText();
    }

    protected override void OnAction(Building building)
    {
        upgradeComponent.StartUpgrading();
    }

    protected override void CreateWidgets(Building building)
    {
        foreach (var buildItem in building.NextLevelDefinition.ResourcesToBuild) {
            var widget = ResourceWidgetFactory.CreateResourceWidget(ResourceWidgetPrefab, LayoutGroup.transform);
            spawnedResourceWidgets.Add(widget);

            var item = cityStorage.Inventory.GetItem(buildItem.Definition.ItemId);

            widget.SetItem(item.Definition);
            widget.AddAmount(item);
            widget.SetLimit(buildItem);
        }
    }

    protected override void UpdateIcon(Building building)
    {
        BuildingImage.sprite = building.NextLevelDefinition.BuildingThumb;
    }

    protected override bool ShouldEnableButton()
    {
        if (!base.ShouldEnableButton()) return false;

        var cityStorage = CityStorage.Instance;
        if (!cityStorage) return false;

        foreach (var buildItem in building.NextLevelDefinition.ResourcesToBuild) {
            var storageItem = cityStorage.Inventory.GetItem(buildItem.Definition.ItemId);
            if (buildItem.Amount > storageItem.Amount) return false;
        }

        return true;
    }

    private void UpdateBuildTimeText()
    {
        if (!buildTimeText) return;

        var levelDefinition = building.NextLevelDefinition;
        if (!levelDefinition) return;

        if (levelDefinition.UpgradeTime > 0) {
            buildTimeText.SetLocalizationItem(buildTimeLocalization);
            buildTimeText.SetPlaceHolderLocalization(levelDefinition);
        }
        else {
            buildTimeText.SetLocalizationItem(instantlyBuildLocalization);
        }
    }
}