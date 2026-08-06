using UnityEngine;

public class WorkersContextElement : ContextElement
{
    [Header("Workers")]
    [SerializeField] private WorkersControlMenu workersMenu;

    protected override void OnButtonClicked()
    {
        workersMenu.Show();
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        var building = target.GetComponent<Building>();
        if (!building) return false;
        if (!building.Definition.IsWorkable) return false;

        var pierModule = building.GetComponent<PierModule>();
        if (building.ConstructionComponent.GetUnderConstruction() && !pierModule) return false;

        return true;
    }
}