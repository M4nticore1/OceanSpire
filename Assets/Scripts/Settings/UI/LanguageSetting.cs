using TMPro;
using UnityEngine;

public class LanguageSetting : MonoBehaviour
{
    [SerializeField] private PlayerSettingsManager playerSettingsManager;
    [SerializeField] private SelectLanguageMenu languageMenu;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI languageName;
    [SerializeField] private LocalizationItem languageNameLocalizationItem;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);
        LocalizationManager.Instance.OnLocalizationChanged += OnLocalizationCahanged;
        UpdateLanguageName();
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
        LocalizationManager.Instance.OnLocalizationChanged -= OnLocalizationCahanged;
    }

    private void UpdateLanguageName()
    {
        var languaeCodee = LocalizationManager.Instance.CurrentLocalization.LanguageCode;
        var text = LocalizationManager.Instance.GetText(languageNameLocalizationItem, languaeCodee);
        languageName.SetText(text);
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