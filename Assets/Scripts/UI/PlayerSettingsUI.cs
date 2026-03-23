using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsUI : MonoBehaviour, IOpenable
{
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Toggle fpsCounterToggle;

    private void OnEnable()
    {
        soundSlider.onValueChanged.AddListener(OnGlobalVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        fpsCounterToggle.onValueChanged.AddListener(OnFpsCounterValueChanged);
    }

    private void OnDisable()
    {
        soundSlider.onValueChanged.RemoveListener(OnGlobalVolumeChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        vSyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
        fpsCounterToggle.onValueChanged.RemoveListener(OnFpsCounterValueChanged);
    }

    private void Start()
    {
        CreateLanguages();
        Close();
    }

    // IOpenable
    public void Open()
    {
        contentRoot.SetActive(true);
    }

    public void Close()
    {
        contentRoot.SetActive(false);
    }

    private void CreateLanguages()
    {
        foreach (var localization in LocalizationsList.Instance.Localizations) {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData() {
                text = localization.Language.ToString(),
            };

            languageDropdown.options.Add(data);
        }

        languageDropdown.RefreshShownValue();
    }

    private void OnGlobalVolumeChanged(float value)
    {
        
    }

    private void OnMusicVolumeChanged(float value)
    {

    }

    private void OnVSyncChanged(bool value)
    {
        QualitySettings.vSyncCount = value ? 2 : 0;
    }

    private void OnFpsCounterValueChanged(bool value)
    {
        FPSCounterSystem.SetCounterEnabled(value);
    }
}
