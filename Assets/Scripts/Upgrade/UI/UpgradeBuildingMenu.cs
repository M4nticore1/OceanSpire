using UnityEngine;

public class UpgradeBuildingMenu : BuildingMenu
{
    [SerializeField] private CityStorage cityStorage;

    private UpgradeComponent upgradeComponent;

    protected override void OnOpened(Building building)
    {
        upgradeComponent = building.UpgradeComponent;
    }

    protected override void OnAction(Building building)
    {
        upgradeComponent.StartUpgrading();
    }

    protected override void CreateWidgets(Building building)
    {
        foreach (var item in building.NextLevelData.ResourcesToBuild) {
            var widget = ResourceWidgetFactory.CreateResourceWidget(ResourceWidgetPrefab, LayoutGroup.transform);
            spawnedResourceWidgets.Add(widget);

            widget.AddAmount(cityStorage.Inventory.GetItem(item.Definition.ItemId));
            widget.SetLimit(item);
        }
    }

    protected override void UpdateIcon(Building building)
    {
        BuildingImage.sprite = building.NextLevelData.BuildingThumb;
    }
}