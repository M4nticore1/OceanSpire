using TMPro;
using UnityEngine;

public class TextLocalizer : MonoBehaviour
{
    LocalizationManager localizationManager;
    private TextMeshProUGUI text;
    [SerializeField] private string key = "";

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        localizationManager = FindAnyObjectByType<LocalizationManager>();
        ChangeLocalization();
    }

    private void OnEnable()
    {
        LocalizationSystem.OnLocalizationChanged += ChangeLocalization;
    }

    private void OnDisable()
    {
        LocalizationSystem.OnLocalizationChanged -= ChangeLocalization;
    }

    private void ChangeLocalization()
    {
        text.SetText(LocalizationSystem.GetLocalizationText(key));
    }
}
