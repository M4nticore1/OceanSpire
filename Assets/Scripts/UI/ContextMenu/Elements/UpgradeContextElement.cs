using UnityEngine;

public class UpgradeContextElement : ContextMenuElement
{
    [SerializeField] private UpgradeActionMenu upgradeMenu;

    protected override void OnShowed()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (building.NextLevelData) return;

        button.SetState(CustomButtonState.Disabled);
        button.FinishTransitionAnimation();
    }

    protected override void OnButtonClicked()
    {
        upgradeMenu.Open();
    }

    protected override bool ShouldShow()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return false;

        return true;
    }
}