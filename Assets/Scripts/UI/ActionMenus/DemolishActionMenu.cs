using UnityEngine;

public class DemolishActionMenu : ActionMenu
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.OnRaidStarted += OnRaidStarted;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.OnRaidStarted -= OnRaidStarted;

        return true;
    }

    protected override void OnOpened()
    {
        actionButton.SetState(RaidManager.Instance.IsRaidStarted ? CustomButtonState.Disabled : CustomButtonState.Idle);
    }

    protected override void OnAction(Building building)
    {
        building.Demolish();
    }

    protected override void CreateWidgets(Building building)
    {
        ItemInstance[] resourcesToBuild = building.GetResourcesToRefund();
        int resourcesCount = resourcesToBuild.Length;
        spawnedResourceWidgets = new ResourceWidget[resourcesCount];

        for (int i = 0; i < resourcesCount; i++) {
            ItemInstance resource = resourcesToBuild[i];

            spawnedResourceWidgets[i] = Instantiate(resourceWidgetPrefab, layoutGroup.transform);
            spawnedResourceWidgets[i].SetAmount(resource);
        }
    }

    private void OnRaidStarted()
    {
        actionButton.SetState(CustomButtonState.Disabled);
    }
}