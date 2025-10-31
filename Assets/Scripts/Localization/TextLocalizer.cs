using TMPro;
using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

public class TextLocalizer : MonoBehaviour
{
    LocalizationManager localizationManager;
    private TextMeshProUGUI text;
    [SerializeField] private string key = "";

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        localizationManager = FindAnyObjectByType<LocalizationManager>();
    }

    private void OnEnable()
    {
        if (localizationManager)
        {
            LocalizationManager.OnLanguageChanged += ChangeLanguage;
        }
        else
            Debug.LogWarning("localizationManager is null");
    }

    private void OnDisable()
    {
        if (localizationManager)
        {

        }
        else
            Debug.LogWarning("localizationManager is null");
    }

    private void ChangeLanguage(Dictionary<string, string> json)
    {
        text.SetText(json[key]);
    }
}
