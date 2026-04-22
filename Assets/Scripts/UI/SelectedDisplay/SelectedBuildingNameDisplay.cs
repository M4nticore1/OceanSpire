using UnityEngine;

public class SelectedBuildingNameDisplay : SelectedDisplay
{
    [SerializeField] protected TextLocalizer localizer;

    protected override void TryDisplay()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        LocalizationItem item = building.BuildingData.LocalizationItem;

        localizer.SetLocalizationItem(item);
        localizer.UpdateText();
    }

    protected override void TryHide()
    {
        localizer.SetText("");
    }
}