using UnityEngine;

public class DemolishContextElement : ContextElement
{
    [SerializeField] private DemolishActionMenu demolishMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        demolishMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        Building building = target.GetComponent<Building>();
        if (!building) return false;

        if (!building.BuildingData.IsDemolishable) return false;

        return true;
    }
}