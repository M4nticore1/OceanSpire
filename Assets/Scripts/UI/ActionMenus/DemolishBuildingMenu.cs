using UnityEngine;

public class DemolishBuildingMenu : BuildingMenu
{
    [SerializeField] private RaidManager raidManager;

    protected override bool TrySubscribe()
    {
        if (!base.TrySubscribe()) return false;

        return true;
    }

    protected override bool TryUnsubscribe()
    {
        if (!base.TryUnsubscribe()) return false;

        return true;
    }

    protected override void OnOpened(Building building)
    {

    }

    protected override void OnAction(Building building)
    {
        building.Demolish();
    }

    protected override void CreateWidgets(Building building)
    {
        var resourcesToBuild = building.GetResourcesToRefund();
        int resourcesCount = resourcesToBuild.Length;

        for (int i = 0; i < resourcesCount; i++) {
            var resource = resourcesToBuild[i];

            var widget = Instantiate(ResourceWidgetPrefab, LayoutGroup.transform);
            spawnedResourceWidgets.Add(widget);
            spawnedResourceWidgets[i].AddAmount(resource);
        }
    }

    protected override void UpdateIcon(Building building)
    {
        BuildingImage.sprite = building.LevelData.BuildingThumb;
    }
}