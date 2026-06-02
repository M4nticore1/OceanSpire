using UnityEngine;

public class ClickableTutorialStep : TutorialStep
{
    [SerializeField] private MonoBehaviour clickable;
    private IClickable Clickable => clickable.GetComponent<IClickable>();

    protected override void OnShow()
    {
        base.OnShow();

        if (Clickable == null) {
            Debug.Log($"Clickable not found at {name}");
            return;
        }

        Clickable.SetClickable(true);
    }
}