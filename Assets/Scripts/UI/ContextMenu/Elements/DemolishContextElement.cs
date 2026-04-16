using UnityEngine;

public class DemolishContextElement : ContextMenuElement
{
    [SerializeField] private DemolishActionMenu demolishMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        demolishMenu.Open();
    }

    protected override bool ShouldShow()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return false;

        if (!building.BuildingData.IsDemolishable) return false;

        return true;
    }
}