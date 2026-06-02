using UnityEngine;

public class DropdownTutorialStep : TutorialStep
{
    [SerializeField] private CustomDropdown dropdown;

    protected override void OnShow()
    {
        base.OnShow();

        dropdown.SetListeningClick(false);
    }

    protected override void OnComplete()
    {
        base.OnComplete();

        dropdown.SetListeningClick(true);
    }
}