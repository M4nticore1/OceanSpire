using UnityEngine;

public class UpgradeBuildingMenu : BuildingMenu
{
    [Header("Upgrade Menu")]
    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private TextLocalizer buildTimeText;
    [SerializeField] private LocalizationItem buildTimeLocalization;
    [SerializeField] private LocalizationItem instantlyBuildLocalization;

    protected override void OnOpened(Building building)
    {
        UpdateBuildTimeText();
    }

    protected override void OnAction(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Building is not valid!");
        }

        building.UpgradeComponent.StartUpgrading();
    }

    protected override void CreateWidgets(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Building is not valid!");
            return;
        }

        var nextLevel = building.NextLevelDefinition;
        if (nextLevel == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Next Level is not valid at {building}!");
            return;
        }

        foreach (var buildItem in nextLevel.ResourcesToBuild) {
            if (buildItem == null) {
                Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Upgrade Resource is not valid at {building}!");
                continue;
            }

            var widget = ResourceWidgetFactory.CreateResourceWidget(ResourceWidgetPrefab, LayoutGroup.transform);
            spawnedResourceWidgets.Add(widget);

            var item = cityStorage.Inventory.GetInventoryItem(buildItem.Definition.ItemId);

            widget.SetItemDefinition(item.Definition);
            widget.AddAmount(item);
            widget.SetLimit(buildItem);
        }
    }

    protected override void UpdateIcon(Building building)
    {
        if (building == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Building is not valid!");
            return;
        }

        var nextLevel = building.NextLevelDefinition;
        if (nextLevel == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Next Level is not valid at {building}!");
            return;
        }

        BuildingImage.sprite = building.NextLevelDefinition.BuildingThumb;
    }

    protected override bool ShouldEnableButton()
    {
        if (!base.ShouldEnableButton()) return false;

        if (building == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Building is not valid!");
            return false;
        }

        var nextLevel = building.NextLevelDefinition;
        if (nextLevel == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] Next Level is not valid at {building}!");
            return false;
        }

        var cityStorage = CityStorage.Instance;
        if (cityStorage == null) {
            Debug.LogError($"[{nameof(UpgradeBuildingMenu)}] City Storage is not valid!");
            return false;
        }

        if (RaidManager.Instance && RaidManager.Instance.IsUnderRaid) {
            return false;
        }

        foreach (var buildItem in building.NextLevelDefinition.ResourcesToBuild) {
            var storageItem = cityStorage.Inventory.GetInventoryItem(buildItem.Definition.ItemId);
            if (buildItem.Amount > storageItem.Amount) return false;
        }

        return true;
    }

    private void UpdateBuildTimeText()
    {
        if (buildTimeText == null) return;

        var levelDefinition = building.NextLevelDefinition;
        if (levelDefinition == null) return;

        if (levelDefinition.UpgradeTime > 0) {
            buildTimeText.SetLocalizationItem(buildTimeLocalization);
            buildTimeText.SetPlaceHolderLocalization(levelDefinition);
        }
        else {
            buildTimeText.SetLocalizationItem(instantlyBuildLocalization);
        }
    }
}