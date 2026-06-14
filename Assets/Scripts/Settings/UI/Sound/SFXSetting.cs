using UnityEngine;

public class SFXSetting : SoundSetting
{
    protected override void OnSliderValueChanged(float value)
    {
        PlayerSettingsManager.SetSFXVolume(value);
    }

    protected override float GetVolumeAlpha()
    {
        return PlayerSettingsManager.SFXVolume;
    }
}