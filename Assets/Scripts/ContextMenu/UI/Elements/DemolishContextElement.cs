using UnityEngine;

public class DemolishContextElement : ContextElement
{
    [Header("Demolish")]
    [SerializeField] private DemolishBuildingMenu demolishMenu;
    private Building building;

    protected override void OnShowed()
    {

    }

    protected override void OnButtonClicked()
    {
        demolishMenu.Open(building);
    }

    protected override bool ShouldShow(ContextMenuTarget target)
    {
        building = target.GetComponent<Building>();
        if (!building) return false;
        if (!building.BuildingData.IsDemolishable) return false;

        return true;
    }
}