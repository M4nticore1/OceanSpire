using System;
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
            }

            return instance;
        }
    }

    public LocalizationTable CurrentLocalization { get; private set; }
    private Dictionary<string, string> deserializedLocalization;

    public event Action<LocalizationTable> OnLocalizationChanged;

    private LocalizationManager() { }

    public void SetLocalization(SystemLanguage language)
    {
        if (!LocalizationsList.Instance.LocalizationsDict.ContainsKey(language)) {
            SetLocalization(SystemLanguage.English);
            return;
        }

        CurrentLocalization = LocalizationsList.Instance.GetLocalization(language);
        deserializedLocalization = JsonConvert.DeserializeObject<Dictionary<string, string>>(CurrentLocalization.LocalizationAsset.text);
        OnLocalizationChanged?.Invoke(CurrentLocalization);
    }

    public string GetText(LocalizationItem item)
    {
        string key = item.name;
        return GetText(key);
    }

    public string GetText(string key)
    {
        if (deserializedLocalization == null) return null;

        if (!deserializedLocalization.TryGetValue(key, out var text)) {
            Debug.LogWarning($"Localization key not found: '{key}'");
            return key;
        }

        return text;
    }

    public TMP_FontAsset GetFont(TextRole role)
    {
        if (!CurrentLocalization) return null;

        int fontIndex = (int)role;
        var font = CurrentLocalization.Fonts[fontIndex];

        return font;
    }

    public string GetLanguageNameByLocalization(SystemLanguage language)
    {
        var textAsset = LocalizationsList.Instance.GetLocalization(language).LocalizationAsset;
        var localiations = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);
        string text = localiations["language_name"];
        return text;
    }
}