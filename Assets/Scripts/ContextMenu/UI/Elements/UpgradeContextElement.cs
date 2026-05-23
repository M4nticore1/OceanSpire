using UnityEngine;

public class UpgradeContextElement : ContextElement
{
    [SerializeField] private UpgradeMenu upgradeMenu;

    private Building building;

    protected override void OnShowed()
    {
        Debug.Log(building.NextLevelData);
        if (building.NextLevelData) return;

        button.SetState(CustomButtonState.Disabled);
        button.EndTransitionAnimation();
    }

    protected override void OnButtonClicked()
    {
        var selectedUpgradeComponent = SelectManager.Instance.SelectedComponent.GetComponent<UpgradeComponent>();
        if (!selectedUpgradeComponent) return;

        upgradeMenu.Open(selectedUpgradeComponent);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (!building) return false;

        if (building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}