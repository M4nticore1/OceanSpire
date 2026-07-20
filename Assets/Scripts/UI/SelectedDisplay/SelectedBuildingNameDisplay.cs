using UnityEngine;

public class SelectedBuildingNameDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer localizer;

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        var building = selectComponent.GetComponent<Building>();
        var item = building.Definition.NameLocalizationItem;

        localizer.SetLocalizationItem(item);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var building = selectComponent.GetComponent<Building>();
        if (!building) return false;

        return true;
    }
}