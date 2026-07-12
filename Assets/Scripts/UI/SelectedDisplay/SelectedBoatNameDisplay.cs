using UnityEngine;

public class SelectedBoatNameDisplay : SelectedDisplay
{
    [Header("Boat Name")]
    [SerializeField] private TextLocalizer textLocalizer;

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

        var boat = selectComponent.GetComponent<Boat>();
        if (!boat) return;

        textLocalizer.SetLocalizationItem(boat.Definition.NameLocalization);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var boat = selectComponent.GetComponent<Boat>();
        if (!boat) return false;

        return true;
    }
}