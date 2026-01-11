using TMPro;
using UnityEngine;

public class TextLocalizer : MonoBehaviour
{
    private GameManager gameManager = null;
    private LocalizationManager LocalizationManager => gameManager.localizationManager;
    private TextMeshProUGUI text;
    private string keyText = "";
    [SerializeField] private string key = "";

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLocalizationChanged += ChangeLocalization;
        if (LocalizationManager.isInitialized && keyText != LocalizationManager.GetLocalizationText(key)) {
            ChangeLocalization();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizationChanged -= ChangeLocalization;
    }

    private void ChangeLocalization()
    {
        Debug.Log(key);
        keyText = LocalizationManager.GetLocalizationText(key);
        text.SetText(keyText);
    }
}
