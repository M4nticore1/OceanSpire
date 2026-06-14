using UnityEngine;

public class FrameRateLimitSetting : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    [SerializeField] private CustomToggle toggle;

    private void OnEnable()
    {
        toggle.OnValueChanged += OnToggleValueChanged;
        UpdateLimit(playerSettingsManager.FrameRateLimitType);
    }

    private void OnDisable()
    {
        toggle.OnValueChanged -= OnToggleValueChanged;
    }

    private void UpdateLimit(int value)
    {
        playerSettingsManager.SetFrameRateLimit(value);
        toggle.SetOn(value == 0 ? false : true);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateLimit(value ? 1 : 0);
    }
}