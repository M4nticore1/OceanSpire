using UnityEngine;

public class SelectedBuildingLevelDisplay : SelectedDisplay
{
    [SerializeField] protected TextLocalizer localizer;
    [SerializeField] private LocalizationItem levelLocalization;
    [SerializeField] private LocalizationItem constructionLocalization;

    private Building building;

    private void Update()
    {
        if (!building) return;
        if (!building.ConstructionComponent.IsUnderConstruction) return;

        localizer.UpdateText();
    }

    protected override void TryDisplay()
    {
        building = SelectManager.Instance.GetSelectedBuilding();
        if (!building) return;

        ILocalizable localizable = building.GetComponent<ILocalizable>();

        if (building.ConstructionComponent.IsUnderConstruction) {
            localizer.SetLocalizationItem(constructionLocalization);
        }
        else {
            localizer.SetLocalizationItem(levelLocalization);
        }

        localizer.SetPlaceHolderLocalization(localizable);
        localizer.UpdateText();
    }

    protected override void TryHide()
    {
        localizer.SetText("");
    }
}