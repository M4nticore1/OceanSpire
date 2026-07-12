using UnityEngine;

public class WorkersContextElement : ContextElement
{
    [Header("Workers")]
    [SerializeField] private WorkersControlMenu workersMenu;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        workersMenu.Open();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        Building building = target.GetComponent<Building>();
        if (!building) return false;

        if (building.ConstructionComponent.GetUnderConstruction()) return false;
        if (!building.BuildingData.IsWorkable) return false;

        return true;
    }
}