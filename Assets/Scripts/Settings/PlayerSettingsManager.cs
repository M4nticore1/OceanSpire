using System;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSettingsManager : MonoBehaviour
{
    public static PlayerSettingsManager Instance;

    [SerializeField] private AudioMixer masterMixer;

    public string LanguageCode { get; private set; }
    public float SFXVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public int FrameRateLimitType { get; private set; }
    public bool ShowFrameRateCounter { get; private set; }

    public event Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Another PlayerSettingsManager is already on the scene");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Init()
    {
        var playerSettingsData = PlayerSettingsData.Default();
        Init(playerSettingsData);
    }

    public void Init(PlayerSettingsData playerSettingsData)
    {
        if (playerSettingsData == null) {
            Debug.LogError($"Player Settings Data is not valid at {name}");
            Init();
            return;
        }

        SetLanguage(playerSettingsData.Language);
        SetSFXVolume(playerSettingsData.SFXVolume);
        SetMusicVolume(playerSettingsData.MusicVolume);
        SetFrameRateLimit(playerSettingsData.FrameRateLimitType);
        SetShowFrameRateCounter(playerSettingsData.ShowFrameRateCounter);
    }

    public void SetLanguage(string languageCode)
    {
        LocalizationManager.Instance.SetLocalization(languageCode);
        LanguageCode = LocalizationManager.Instance.CurrentLocalization.LanguageCode;

        OnSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        SFXVolume = alpha;
        float volume = GetVolumeFromAlpha(alpha);
        masterMixer.SetFloat("SFX", volume);

        OnSettingsChanged?.Invoke();
    }

    public void SetMusicVolume(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        MusicVolume = alpha;
        float volume = GetVolumeFromAlpha(alpha);
        masterMixer.SetFloat("Music", volume);

        OnSettingsChanged?.Invoke();
    }

    public void SetFrameRateLimit(int type)
    {
        FrameRateLimitType = type;
        Application.targetFrameRate = type == 0 ? 30 : 60;

        OnSettingsChanged?.Invoke();
    }

    public void SetShowFrameRateCounter(bool value)
    {
        ShowFrameRateCounter = value;
        FPSCounterSystem.SetCounterEnabled(value);

        OnSettingsChanged?.Invoke();
    }

    private float GetVolumeFromAlpha(float alpha)
    {
        alpha = Mathf.Max(0.0001f, alpha);
        alpha = Mathf.Log10(alpha) * 20;

        return alpha;
    }
}