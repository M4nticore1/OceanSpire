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

    public static PlayerSettingsData Create(PlayerSettingsManager playerSettings)
    {
        return new PlayerSettingsData()
        {
            Language = playerSettings.Language,
            SFXVolume = playerSettings.SFXVolume,
            MusicVolume = playerSettings.MusicVolume,
            FrameRateLimitType = playerSettings.FrameRateLimitType,
            ShowFrameRateCounter = playerSettings.ShowFrameRateCounter
        };
    }

    public static PlayerSettingsData Default()
    {
        return new PlayerSettingsData()
        {
            Language = Application.systemLanguage,
            SFXVolume = 0.5f,
            MusicVolume = 0.5f,
            FrameRateLimitType = 60,
            ShowFrameRateCounter = true
        };
    }
}