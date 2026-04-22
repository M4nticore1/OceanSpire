using UnityEngine;

public class SelectedBuildingLevelDisplay : SelectedDisplay
{
    [SerializeField] protected TextLocalizer localizer;
    [SerializeField] private LocalizationItem levelLocalization;

    protected override void TryDisplay()
    {
        Building building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        ILocalizable localizable = building.GetComponent<ILocalizable>();
        LocalizationItem item = building.BuildingData.LocalizationItem;

        localizer.SetLocalizationItem(levelLocalization);
        localizer.SetPlaceHolderLocalization(localizable);
        localizer.UpdateText();
    }

    protected override void TryHide()
    {
        localizer.SetText("");
    }
}