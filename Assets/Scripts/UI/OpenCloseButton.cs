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

    private void Awake()
    {
        if (TargetOpenable == null) {
            Debug.LogError($"[{nameof(OpenCloseButton)}] IOpenable is not valid at {targetOpenable}!");
        }
        if (button == null) {
            Debug.LogError($"[{nameof(OpenCloseButton)}] Button is not valid at {this}!");
        }
    }

    private void OnEnable()
    {
        if (TargetOpenable == null) return;
        if (button == null) return;

        if (buttonType == ToggleButtonType.Open)
            button.OnReleased.AddListener(TargetOpenable.Show);
        else
            button.OnReleased.AddListener(TargetOpenable.Hide);
    }

    private void OnDisable()
    {
        if (TargetOpenable == null) return;
        if (button == null) return;

        if (buttonType == ToggleButtonType.Open)
            button.OnReleased.RemoveListener(TargetOpenable.Show);
        else
            button.OnReleased.RemoveListener(TargetOpenable.Hide);
    }
}
