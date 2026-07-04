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

    public event Action OnLocalizationChanged;

    private LocalizationManager()
    {
        SetLocalization("en-US");
    }

    public void SetLocalization(string languageCoded)
    {
        var localizationTable = LocalizationsList.Instance.GetLocalization(languageCoded);
        if (!localizationTable) {
            SetLocalization("en-US");
            return;
        }

        CurrentLocalization = localizationTable;
        deserializedLocalization = JsonConvert.DeserializeObject<Dictionary<string, string>>(CurrentLocalization.LocalizationAsset.text);
        OnLocalizationChanged?.Invoke();
    }

    public string GetText(LocalizationItem item, string languageCode = null)
    {
        if (!item) {
            Debug.LogError("LocalizationItem is not valid");
            return null;
        }

        if (languageCode == null) {
            languageCode = CurrentLocalization.LanguageCode;
        }

        return GetText(item.name, languageCode);
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

    public string GetText(string key, string languageCode = null)
    {
        if (languageCode == null) {
            languageCode = CurrentLocalization.LanguageCode;
        }

        var localization = GetDeserializedLocalization(languageCode);
        if (localization == null) {
            Debug.LogError($"Can not get deserialized localization of {localization}");
            return GetText(key, "en-US");
        }

        if (!localization.TryGetValue(key, out var text)) {
            Debug.LogError($"Localization {languageCode} key not found: '{key}'");
            return GetText(key, "en-US");
        }

        return text;
    }

    public TMP_FontAsset GetFont(TextRole role, string languageCode = null)
    {
        if (languageCode == null) {
            languageCode = CurrentLocalization.LanguageCode;
        }

        var fontIndex = (int)role;
        var font = LocalizationsList.Instance.GetLocalization(languageCode).Fonts[fontIndex];

        return font;
    }

    public string GetLanguageNameByLocalization(string languageCode)
    {
        var textAsset = LocalizationsList.Instance.GetLocalization(languageCode).LocalizationAsset;
        var localiations = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);

        return localiations["language_name"];
    }

    private Dictionary<string, string> GetDeserializedLocalization(string languageCode)
    {
        var localizationTable = LocalizationsList.Instance.GetLocalization(languageCode);
        if (!localizationTable) return null;

        return JsonConvert.DeserializeObject<Dictionary<string, string>>(localizationTable.LocalizationAsset.text);
    }
}