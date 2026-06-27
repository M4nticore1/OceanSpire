using System;
using UnityEngine;

[Serializable]
public class PlayerSettingsData
{
    public SystemLanguage Language = SystemLanguage.English;
    public float SFXVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public int FrameRateLimitType = 1;
    public bool ShowFrameRateCounter = false;

    public static PlayerSettingsData Default()
    {
        return new PlayerSettingsData()
        {
            Language = Application.systemLanguage,
        };
    }

    public static PlayerSettingsData Create(PlayerSettingsManager playerSettingsManager)
    {
        return new PlayerSettingsData()
        {
            Language = playerSettingsManager.Language,
            SFXVolume = playerSettingsManager.SFXVolume,
            MusicVolume = playerSettingsManager.MusicVolume,
            FrameRateLimitType = playerSettingsManager.FrameRateLimitType,
            ShowFrameRateCounter = playerSettingsManager.ShowFrameRateCounter,
        };
    }
}