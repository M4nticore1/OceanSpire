using TMPro;
using UnityEngine;

public class LanguageSetting : SettingWidget
{
    [SerializeField] private SelectLanguageMenu languageMenu;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI languageName;
    [SerializeField] private LocalizationItem languageNameLocalizationItem;

    private LocalizationManager localizationManager => LocalizationManager.Instance;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);

        if (localizationManager != null) {
            localizationManager.OnLocalizationChanged += OnLocalizationCahanged;
        }

        UpdateLanguageName();
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);

        if (localizationManager != null) {
            localizationManager.OnLocalizationChanged -= OnLocalizationCahanged;
        }
    }

    private void UpdateLanguageName()
    {
        if (localizationManager == null) return;

        var languaeCode = localizationManager.CurrentLocalization.LanguageCode;
        languageName.text = localizationManager.GetLocalizedText(languageNameLocalizationItem, languaeCode);
        languageName.font = localizationManager.GetFont(TextRole.Default, languaeCode);
    }

    private void OnButtonClicked()
    {
        languageMenu.Show();
    }

    private void OnLocalizationCahanged()
    {
        UpdateLanguageName();
    }
}