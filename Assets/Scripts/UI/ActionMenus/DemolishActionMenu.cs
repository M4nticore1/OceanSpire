using UnityEngine;

public class DemolishActionMenu : BuildingActionMenu
{
    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onClickedContextDemolishButton += OnContextClickedButton;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onClickedContextDemolishButton -= OnContextClickedButton;
    }

    protected override void OnAction(Building building)
    {
        building.Demolish();
    }

    protected override void CreateWidgets(Building building)
    {
        ItemInstance[] resourcesToBuild = building.GetResourcesToBuild();
        int resourcesCount = resourcesToBuild.Length;
        spawnedResourceWidgets = new ResourceWidget[resourcesCount];

        for (int i = 0; i < resourcesCount; i++) {
            ItemInstance resource = resourcesToBuild[i];
            spawnedResourceWidgets[i] = Instantiate(resourceWidgetPrefab, layoutGroup.transform);
            spawnedResourceWidgets[i].Init(resource);
        }
    }
}
