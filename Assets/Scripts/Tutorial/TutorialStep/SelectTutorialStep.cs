using UnityEngine;

public class SelectTutorialStep : TutorialStep
{
    [SerializeField] private SelectComponent selectComponent;

    protected override void OnShow()
    {
        base.OnShow();

        selectComponent.IsClickable = true;
    }
}