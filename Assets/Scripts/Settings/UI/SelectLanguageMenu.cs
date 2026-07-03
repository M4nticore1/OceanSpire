using UnityEngine;
using UnityEngine.UI;

public class SelectLanguageMenu : MonoBehaviour
{
    [SerializeField] private LanguageSwitcher languageSwitcherPrefab;
    [SerializeField] private LocalizationsList localizationsList;
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private SelectGroup selectGroup;
    [SerializeField] private CustomButton closeButton;

    private void OnEnable()
    {
        closeButton.OnReleased.AddListener(OnCloseButtonClicked);
    }

    private void OnDisable()
    {
        closeButton.OnReleased.RemoveListener(OnCloseButtonClicked);
    }

    private void Start()
    {
        CreateLanguageSwitchers();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CreateLanguageSwitchers()
    {
        foreach (var localization in localizationsList.Localizations) {
            var widget = Instantiate(languageSwitcherPrefab, layoutGroup.transform);
            widget.Init(localization.LanguageCode, selectGroup);
        }
    }

    private void OnCloseButtonClicked()
    {
        Hide();
    }
}