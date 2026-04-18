using UnityEngine;

public class SelectedBuildingNameDisplay : SelectedDisplay
{
    protected override void Display()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        LocalizationItem item = building.BuildingData.LocalizationItem;

        localizer.SetLocalizationItem(item);
        localizer.UpdateText();
    }
}