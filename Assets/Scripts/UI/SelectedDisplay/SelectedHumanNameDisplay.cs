using UnityEngine;

public class SelectedHumanNameDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer text;

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var human = selectComponent.GetComponent<Human>();
        if (!human) return false;

        return true;
    }

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

        var human = selectComponent.GetComponent<Human>();

        text.SetPlaceHolderLocalization(human.NameComponent);
        text.UpdateText();
    }
}