using UnityEngine;

public class SelectedNameDisplay : SelectedDisplay
{
    [Header("Name Display")]
    [SerializeField] private TextLocalizer textLocalizer;

    private ILocalizable localizable;

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);
        
        textLocalizer.SetPlaceHolderLocalization(localizable);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        foreach (var localization in selectComponent.GetComponents<ILocalizable>()) {
            if (!localization.GetLocalization().ContainsKey("name")) continue;

            localizable = localization;
            return true;
        }

        return false;
    }
}