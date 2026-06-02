using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null) {
                instance = new LocalizationManager();
                instance.Init(null);
            }

            return instance;
        }
    }

    public Dictionary<SystemLanguage, LocalizationTable> localizations { get; private set; } = new Dictionary<SystemLanguage, LocalizationTable>();
    public LocalizationTable currentLocalization { get; private set; }
    private Dictionary<string, string> deserializedLocalization;

    private bool isInited = false;
    public event System.Action OnLocalizationChanged;

    private LocalizationManager() { }

    private void Init(SettingsData data)
    {
        if (isInited) return;

        foreach (var localization in LocalizationsList.Instance.Localizations) {
            localizations.Add(localization.Language, localization);
        }
        Debug.Log("Loaded " + localizations.Count + " localizations");

        if (data != null) {
            SetLocalization(data.language);
        }
        else {
            SystemLanguage systemLanguage = SystemLanguage.Russian;

            if (localizations.ContainsKey(systemLanguage)) {
                SetLocalization(systemLanguage);
            }
            else {
                SetLocalization(SystemLanguage.English);
            }
        }

        isInited = true;
    }

    public void SetLocalization(int value)
    {
        SystemLanguage language = localizations.Values.ToArray()[value].Language;
        SetLocalization(language);
    }

    public void SetLocalization(SystemLanguage language)
    {
        currentLocalization = localizations[language];
        deserializedLocalization = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentLocalization.LocalizationAsset.text);

        OnLocalizationChanged?.Invoke();
    }

    public string GetText(LocalizationItem item)
    {
        string key = item.name;
        return GetText(key);
    }

    public string GetText(string key)
    {
        if (!deserializedLocalization.TryGetValue(key, out var text)) {
            Debug.LogWarning($"Localization key not found: '{key}'");
            return key;
        }

        return text;
    }

    public TMP_FontAsset GetFont(TextRole role)
    {
        int fontIndex = (int)role;
        TMP_FontAsset font = currentLocalization.Fonts[fontIndex];
        return font;
    }

    public string GetLanguageNameByLocalization(SystemLanguage language)
    {
        TextAsset textAsset = localizations[language].LocalizationAsset;
        Dictionary<string, string> localiations = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);
        string text = localiations["language_name"];
        return text;
    }
}