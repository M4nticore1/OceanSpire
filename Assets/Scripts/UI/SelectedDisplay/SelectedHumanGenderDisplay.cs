using UnityEngine;
using UnityEngine.UI;

public class SelectedHumanGenderDisplay : SelectedDisplay
{
    [SerializeField] private Image genderImage;

    protected override void OnShow(SelectComponent selectComponent)
    {
        base.OnShow(selectComponent);

        var genderComponent = selectComponent.GetComponent<GenderComponent>();
        genderImage.sprite = genderComponent.GetGenderSprite();
    }

    protected override bool ShouldDisplay(SelectComponent selectComponent)
    {
        if (!selectComponent) return false;

        var genderComponent = selectComponent.GetComponent<GenderComponent>();
        if (!genderComponent) return false;

        return true;
    }
}