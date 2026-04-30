using UnityEngine;

public class DemolishActionMenu : ActionMenu
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.onRaidStarted += OnRaidStarted;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.onRaidStarted -= OnRaidStarted;

        return true;
    }

    protected override void OnOpened()
    {
        actionButton.SetState(RaidManager.Instance.IsUnderRaid ? CustomButtonState.Disabled : CustomButtonState.Idle);
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
            spawnedResourceWidgets[i].SetAmountItem(resource);
        }
    }

    private void OnRaidStarted()
    {
        actionButton.SetState(CustomButtonState.Disabled);
    }
}