using UnityEngine;

public class SelectedHumanNameDisplay : SelectedDisplay
{
    [SerializeField] private TextLocalizer text;

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        var human = selectComponent.GetComponent<Human>();

        text.SetPlaceHolderLocalization(human.NameComponent);
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var human = selectComponent.GetComponent<Human>();
        if (!human) return false;

        return true;
    }
}