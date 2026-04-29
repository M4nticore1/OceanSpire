using UnityEngine;

public class CompleteConstructionContextElement : ContextMenuElement
{
    [SerializeField] private CompleteConstructionMenu speedUpConstructionMenu;
    private Building building;

    protected override void OnShowed()
    {
        
    }

    protected override void OnButtonClicked()
    {
        speedUpConstructionMenu.Open(building);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (!building) return false;

        if (!building.ConstructionComponent.IsUnderConstruction) return false;

        return true;
    }
}