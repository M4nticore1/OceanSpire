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
        if (!building) {
            Debug.LogError($"[{nameof(DemolishBuildingMenu)}] Building is not valid!");
            return;
        }

        var resourcesToBuild = building.GetResourcesToRefund();
        int resourcesCount = resourcesToBuild.Length;

        foreach (var item in resourcesToBuild) {
            if (item == null) {
                Debug.LogError($"[{nameof(DemolishBuildingMenu)}] Refund resourcec is not valid at {building}!");
                continue;
            }

            var widget = Instantiate(ResourceWidgetPrefab, LayoutGroup.transform);
            spawnedResourceWidgets.Add(widget);

            widget.SetItemDefinition(item.Definition);
            widget.AddAmount(item);
        }
    }

    protected override void UpdateIcon(Building building)
    {
        BuildingImage.sprite = building.LevelDefinition.BuildingThumb;
    }
}