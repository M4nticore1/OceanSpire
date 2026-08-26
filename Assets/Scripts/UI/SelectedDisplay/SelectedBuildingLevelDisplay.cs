using UnityEngine;

public class SelectedBuildingLevelDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer localizer;
    [SerializeField] private LocalizationItem levelLocalization;
    [SerializeField] private LocalizationItem constructionLocalization;

    private Building building;

    private void Update()
    {
        if (building == null) return;
        if (!building.ConstructionComponent.GetUnderConstruction()) return;

        localizer.UpdateText();
    }

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        ILocalizable localizable = building.GetComponent<ILocalizable>();

        if (building.ConstructionComponent.GetUnderConstruction()) {
            localizer.SetLocalizationItem(constructionLocalization);
        }
        else {
            localizer.SetLocalizationItem(levelLocalization);
        }

        localizer.SetPlaceHolderLocalization(localizable);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        building = selectComponent.GetComponent<Building>();
        if (building == null) return false;

        return true;
    }
}