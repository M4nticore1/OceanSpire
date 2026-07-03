using System;
using UnityEngine;

[Serializable]
public class PlayerSettingsData
{
    public string Language = "en-US";
    public float SFXVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public int FrameRateLimitType = 1;
    public bool ShowFrameRateCounter = false;

    public static PlayerSettingsData Default()
    {
        return new PlayerSettingsData()
        {
            Language = LanguageDetector.GetSystemGameLanguage(),
        };
    }

    public static PlayerSettingsData Create(PlayerSettingsManager playerSettingsManager)
    {
        return new PlayerSettingsData()
        {
            Language = playerSettingsManager.LanguageCode,
            SFXVolume = playerSettingsManager.SFXVolume,
            MusicVolume = playerSettingsManager.MusicVolume,
            FrameRateLimitType = playerSettingsManager.FrameRateLimitType,
            ShowFrameRateCounter = playerSettingsManager.ShowFrameRateCounter,
        };
    }
}