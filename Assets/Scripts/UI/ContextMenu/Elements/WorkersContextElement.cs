using UnityEngine;

public class WorkersContextElement : ContextMenuElement
{
    [SerializeField] private WorkersControlMenu workersMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        workersMenu.Open();
    }

    protected override bool ShouldShow()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return false;

        if (building.ConstructionComponent.IsUnderConstruction) return false;
        if (!building.BuildingData.IsWorkable) return false;

        return true;
    }
}