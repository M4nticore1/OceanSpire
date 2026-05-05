using UnityEngine;

public class UpgradeContextElement : ContextMenuElement
{
    [SerializeField] private UpgradeActionMenu upgradeMenu;

    private Building building;

    protected override void OnShowed()
    {
        if (building.NextLevelData) return;

        button.SetState(CustomButtonState.Disabled);
        button.EndTransitionAnimation();
    }

    protected override void OnButtonClicked()
    {
        upgradeMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (!building) return false;

        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}