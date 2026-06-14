using UnityEngine;

public class FrameRateCounterSetting : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    [SerializeField] private CustomToggle toggle;

    private void OnEnable()
    {
        toggle.OnValueChanged += OnToggleValueChanged;
        UpdateCounterEnabled(playerSettingsManager.ShowFrameRateCounter);
    }

    private void OnDisable()
    {
        toggle.OnValueChanged -= OnToggleValueChanged;
    }

    private void UpdateCounterEnabled(bool value)
    {
        playerSettingsManager.SetShowFrameRateCounter(value);
        toggle.SetOn(value);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateCounterEnabled(value);
    }
}