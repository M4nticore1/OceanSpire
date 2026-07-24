using UnityEngine;

public class UpgradeContextElement : ContextElement
{
    [Header("Upgrade")]
    [SerializeField] private UpgradeBuildingMenu upgradeMenu;
    private Building building;

    protected override bool ShouldEnableButton()
    {
        if (!base.ShouldEnableButton()) return false;

        if (!building) return false;
        if (!building.NextLevelDefinition) return false;

        return true;
    }

    protected override void OnButtonClicked()
    {
        var selectedBuilding = SelectManager.Instance.SelectedComponent.GetComponent<Building>();
        if (!selectedBuilding) return;

        upgradeMenu.Open(selectedBuilding);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        if (!target) return false;

        building = target.GetComponent<Building>();
        if (!building) return false;
        if (building.ConstructionComponent.GetUnderConstruction()) return false;

        return true;
    }
}