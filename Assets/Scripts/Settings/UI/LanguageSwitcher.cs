using TMPro;
using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    private SystemLanguage language;
    [SerializeField] private CustomButton button;
    [SerializeField] private TextMeshProUGUI languageName;

    private void OnEnable()
    {
        button.OnReleased.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.OnReleased.RemoveListener(OnButtonClicked);
    }

    public void Init(SystemLanguage language, SelectGroup selectGroup)
    {
        this.language = language;
        UpdateLanguageName(language);
        SetSelectGroup(selectGroup);
        UpdateSelected(language);
    }

    private void UpdateLanguageName(SystemLanguage language)
    {
        string text = LanguageNameTranslater.GetNativeLanguageName(language);
        languageName.SetText(text);
    }

    private void UpdateSelected(SystemLanguage language)
    {
        if (PlayerSettingsManager.Instance.Language != language) return;

        button.SetState(CustomButtonState.Selected);
    }

    private void SetSelectGroup(SelectGroup selectGroup)
    {
        button.SetSelectGroup(selectGroup);
    }

    private void OnButtonClicked()
    {
        PlayerSettingsManager.Instance.SetLanguage(language);
    }
}