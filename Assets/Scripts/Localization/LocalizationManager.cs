using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

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
        if (!item) {
            Debug.LogError("LocalizationItem is not valid");
            return null;
        }

        return GetText(item.name);
    }

    public string GetText(LocalizationItem item, ILocalizable localizable)
    {
        var text = GetText(item);
        if (text == null) return null;

        if (localizable == null) {
            Debug.LogError("localizable is not valid");
            return text;
        }

        var dict = localizable.GetLocalization();
        if (dict == null) return null;

        foreach (var key in dict.Keys.ToArray()) {
            string holder = "{" + key + "}";
            string value = dict[key];
            text = text.Replace(holder, value);
        }

        return text;
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