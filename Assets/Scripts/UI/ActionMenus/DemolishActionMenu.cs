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

    protected override void OnOpen(Building building)
    {
        foreach (var item in building.LevelData.ResourcesToBuild) {

        }
    }
}
