using UnityEngine;

public class ButtonTutorialStep : TutorialStep
{
    [SerializeField] private CustomButton button;

    protected override void OnShow()
    {
        base.OnShow();

        button.SetInteractable(true);
    }
}