using UnityEngine;

public class SFXSetting : SoundSetting
{
    protected override void OnSliderValueChanged(float value)
    {
        if (!playerSettingsManager) return;

        playerSettingsManager.SetSFXVolume(value);
    }

    protected override float GetVolumeAlpha()
    {
        if (!playerSettingsManager) return 0.5f;

        return playerSettingsManager.SFXVolume;
    }
}