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
        if (!building) return;

        upgradeMenu.Open(building);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        if (!target) {
            building = null;
            return false;
        }

        building = target.GetComponent<Building>();
        if (!building) return false;
        if (building.ConstructionComponent.GetUnderConstruction()) return false;

        return true;
    }
}