using UnityEngine;

public class WorkersContextElement : ContextElement
{
    [Header("Workers")]
    [SerializeField] private WorkersControlMenu workersMenu;

    private Building building;

    protected override void OnButtonClicked()
    {
        workersMenu.Show(building);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (building == null) return false;
        if (!building.Definition.IsWorkable) return false;

        var pierModule = building.GetComponent<PierModule>();
        if (building.ConstructionComponent.GetUnderConstruction() && pierModule == null) return false;

        return true;
    }
}