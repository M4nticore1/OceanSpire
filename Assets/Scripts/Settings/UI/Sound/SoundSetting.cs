using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class SoundSetting : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    public PlayerSettingsManager PlayerSettingsManager => playerSettingsManager;

    [SerializeField] private Slider slider;

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        UpdateVolumeText();
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    protected abstract void OnSliderValueChanged(float value);

    protected abstract float GetVolumeAlpha();

    private void UpdateVolumeText()
    {
        slider.value = GetVolumeAlpha();
    }
}