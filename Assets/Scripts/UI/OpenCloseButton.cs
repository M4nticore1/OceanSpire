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
            button.OnReleased.AddListener(TargetOpenable.Open);
        else
            button.OnReleased.AddListener(TargetOpenable.Close);
    }

    private void OnDisable()
    {
        if (!targetOpenable) return;
        if (!button) return;

        if (buttonType == ToggleButtonType.Open)
            button.OnReleased.RemoveListener(TargetOpenable.Open);
        else
            button.OnReleased.RemoveListener(TargetOpenable.Close);
    }
}
