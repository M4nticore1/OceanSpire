using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSettings
{
    private static PlayerSettings _instance;
    public static PlayerSettings Instance => _instance ??= new PlayerSettings();

    private PlayerSettings()
    {
        
    }

    // Graphics
    public bool isPostProcessingEnabled = true;

    // Volume
    public float volume { get; private set; } = 1f;

    // Localization
    public string currentLanguageKey { get; private set; } = "en";

    public void SetFrameRateLimit(float value)
    {
        int frameRate = (int)math.round(value);
        Application.targetFrameRate = frameRate;
    }

    public void SetVSyncCount(int value)
    {
        QualitySettings.vSyncCount = value;
    }

    public void SetPostProcessingEnabled(Volume postProcess)
    {

    }

    //public static void ChangeLanguage(string languageKey)
    //{
    //    LocalizationManager.SetLocalization(languageKey);
    //}
}
