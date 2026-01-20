using TMPro;
using UnityEngine;

public class TextLocalizer : MonoBehaviour
{
    //private LocalizationManager LocalizationManager => gameManager.localizationManager;
    private TextMeshProUGUI textBlock;
    [SerializeField] private string key = "";

    private void Awake()
    {
        textBlock = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += ChangeLocalization;
        if (LocalizationManager.Instance.isInitialized && textBlock.text != LocalizationManager.Instance.GetLocalizationText(key)) {
            ChangeLocalization();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= ChangeLocalization;
    }

    private void ChangeLocalization()
    {
        string localize = LocalizationManager.Instance.GetLocalizationText(key);
        string text;
        if (localize != "")
            text = localize;
        else
            text = key;
        textBlock.SetText(text);
    }
}
