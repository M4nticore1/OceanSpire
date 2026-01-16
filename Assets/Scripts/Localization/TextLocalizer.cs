using TMPro;
using UnityEngine;

public class TextLocalizer : MonoBehaviour
{
    //private LocalizationManager LocalizationManager => gameManager.localizationManager;
    private TextMeshProUGUI text;
    private string keyText = "";
    [SerializeField] private string key = "";

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLocalizationChanged += ChangeLocalization;
        if (LocalizationManager.Instance.isInitialized && keyText != LocalizationManager.Instance.GetLocalizationText(key)) {
            ChangeLocalization();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLocalizationChanged -= ChangeLocalization;
    }

    private void ChangeLocalization()
    {
        Debug.Log(key);
        keyText = LocalizationManager.Instance.GetLocalizationText(key);
        text.SetText(keyText);
    }
}
