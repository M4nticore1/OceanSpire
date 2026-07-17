using UnityEngine;

public class FrameRateLimitSetting : SettingWidget
{
    [SerializeField] private CustomToggle toggle;

    private void OnEnable()
    {
        toggle.OnValueChanged += OnToggleValueChanged;

        if (playerSettingsManager) {
            UpdateLimit(playerSettingsManager.FrameRateLimitType);
        }
    }

    private void OnDisable()
    {
        toggle.OnValueChanged -= OnToggleValueChanged;
    }

    private void UpdateLimit(int value)
    {
        if (!playerSettingsManager) return;

        playerSettingsManager.SetFrameRateLimit(value);
        toggle.SetOn(value == 0 ? false : true);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateLimit(value ? 1 : 0);
    }
}