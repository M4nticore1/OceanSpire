using UnityEngine;

public class FrameRateCounterSetting : SettingWidget
{
    [SerializeField] private CustomToggle toggle;

    private void OnEnable()
    {
        toggle.OnValueChanged += OnToggleValueChanged;

        if (playerSettingsManager) {
            UpdateCounterEnabled(playerSettingsManager.ShowFrameRateCounter);
        }
    }

    private void OnDisable()
    {
        toggle.OnValueChanged -= OnToggleValueChanged;
    }

    private void UpdateCounterEnabled(bool value)
    {
        if (!playerSettingsManager) return;

        playerSettingsManager.SetShowFrameRateCounter(value);
        toggle.SetOn(value);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateCounterEnabled(value);
    }
}