using UnityEngine;

public class UpgradeContextElement : ContextElement
{
    [SerializeField] private UpgradeMenu upgradeMenu;

    private Building building;

    protected override void OnShowed()
    {
        button.SetState(building.NextLevelData ? CustomButtonState.Idle : CustomButtonState.Disabled);
        button.EndTransitionAnimation();
    }

    protected override void OnButtonClicked()
    {
        var selectedBuilding = SelectManager.Instance.SelectedComponent.GetComponent<Building>();
        if (!selectedBuilding) return;

        upgradeMenu.Open(selectedBuilding);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (!building) return false;

        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}