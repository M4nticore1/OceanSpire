using UnityEngine;

public class SelectedBuildingLevelDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer localizer;
    [SerializeField] private LocalizationItem levelLocalization;
    [SerializeField] private LocalizationItem constructionLocalization;

    private Building building;

    private void Update()
    {
        if (!building) return;
        if (!building.ConstructionComponent.IsUnderConstruction) return;

        localizer.UpdateText();
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        building = selectComponent.GetComponent<Building>();
        if (!building) return false;

        return true;
    }

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

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
}