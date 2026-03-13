using UnityEngine;

public class UpgradeActionMenu : BuildingActionMenu
{
    protected override void OnEnable()
    {
        EventBus.onClickedContextUpgradeButton += OnContextClickedButton;
    }

    protected override void OnDisable()
    {
        EventBus.onClickedContextUpgradeButton -= OnContextClickedButton;
    }

    protected override void OnAction(Building building)
    {

    }

    protected override void CreateWidgets(Building building)
    {
        foreach (var item in building.NextLevelData.ResourcesToBuild) {

        }
    }
}
