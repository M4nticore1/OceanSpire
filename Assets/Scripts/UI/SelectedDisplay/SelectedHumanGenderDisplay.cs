using UnityEngine;
using UnityEngine.UI;

public class SelectedHumanGenderDisplay : SelectedDisplay
{
    [SerializeField] private Image genderImage;

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var genderComponent = selectComponent.GetComponent<GenderComponent>();
        if (!genderComponent) return false;

        return true;
    }

    protected override void Display(SelectComponent selectComponent)
    {
        base.Display(selectComponent);

        var genderComponent = selectComponent.GetComponent<GenderComponent>();
        genderImage.sprite = genderComponent.GetGenderSprite();
    }
}