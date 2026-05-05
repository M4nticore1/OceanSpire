using UnityEngine;

public enum ToggleButtonType
{
    Open,
    Close
}

public class OpenCloseButton : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetOpenable;
    private IOpenable TargetOpenable => targetOpenable ? targetOpenable as IOpenable : null;

    [SerializeField] private ToggleButtonType buttonType;
    [SerializeField] private CustomButton button;

    private void OnEnable()
    {
        if (!targetOpenable) return;
        if (!button) return;

        if (buttonType == ToggleButtonType.Open)
            button.onReleased.AddListener(TargetOpenable.Open);
        else
            button.onReleased.AddListener(TargetOpenable.Close);
    }

    private void OnDisable()
    {
        if (!targetOpenable) return;
        if (!button) return;

        if (buttonType == ToggleButtonType.Open)
            button.onReleased.RemoveListener(TargetOpenable.Open);
        else
            button.onReleased.RemoveListener(TargetOpenable.Close);
    }
}
