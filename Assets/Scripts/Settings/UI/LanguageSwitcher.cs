using TMPro;
using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    private string languageCode = string.Empty;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI languageNameText;
    [SerializeField] private LocalizationItem languageNameLocalizationItem;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);
        UpdateSelected(languageCode);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    public void Init(string languageCode, SelectGroup selectGroup)
    {
        this.languageCode = languageCode;
        UpdateLanguageName(languageCode);
        SetSelectGroup(selectGroup);
        UpdateSelected(languageCode);
    }

    private void UpdateLanguageName(string languageCode)
    {
        var text = LocalizationManager.Instance.GetLocalizedText(languageNameLocalizationItem, languageCode);
        var font = LocalizationManager.Instance.GetFont(TextRole.Default, languageCode);

        languageNameText.SetText(text);
        languageNameText.font = font;
    }

    private void UpdateSelected(string languageCode)
    {
        if (languageCode == string.Empty) return;
        if (LocalizationManager.Instance.CurrentLocalization.LanguageCode != languageCode) return;

        button.SetState(CustomButtonState.Selected);
    }

    private void SetSelectGroup(SelectGroup selectGroup)
    {
        button.SetSelectGroup(selectGroup);
    }

    private void OnButtonClicked()
    {
        PlayerSettingsManager.Instance.SetLanguage(languageCode);
    }
}