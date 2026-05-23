using UnityEngine;

public class SelectedBuildingNameDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer localizer;

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var building = selectComponent.GetComponent<Building>();
        if (!building) return false;

        return true;
    }

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

        var building = selectComponent.GetComponent<Building>();
        var item = building.BuildingData.NameLocalizationItem;

        localizer.SetLocalizationItem(item);
        localizer.UpdateText();
    }
}