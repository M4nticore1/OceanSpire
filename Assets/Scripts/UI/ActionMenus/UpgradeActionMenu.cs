using UnityEngine;

public class UpgradeActionMenu : ActionMenu
{
    protected override void OnOpened()
    {

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