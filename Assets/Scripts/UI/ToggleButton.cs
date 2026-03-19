using UnityEngine;

public enum ToggleButtonType
{
    Open,
    Close
}

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private ToggleButtonType buttonType;
    [SerializeField] private MonoBehaviour targetOpenable;
    private IOpenable TargetOpenable => targetOpenable as IOpenable;
    [SerializeField] private CustomButton button;

    private void OnEnable()
    {
        if (buttonType == ToggleButtonType.Open)
            button.onReleased += TargetOpenable.Open;
        else
            button.onReleased += TargetOpenable.Close;
    }

    private void OnDisable()
    {
        if (buttonType == ToggleButtonType.Open)
            button.onReleased -= TargetOpenable.Open;
        else
            button.onReleased -= TargetOpenable.Close;
    }
}
