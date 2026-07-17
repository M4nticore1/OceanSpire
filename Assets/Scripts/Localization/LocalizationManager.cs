using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance
    {
        get {
            if (instance == null) {
                instance = new LocalizationManager();
            }
            return instance;
        }
    }

    public LocalizationTable CurrentLocalization { get; private set; }
    public event Action OnLocalizationChanged;

    private LocalizationsList localizationsList => LocalizationsList.Instance;

    private LocalizationManager()
    {
        SetLocalization("en-US");
    }

    public void SetLocalization(string languageCode)
    {
        if (!localizationsList) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localizations List is not valid!");
            return;
        }

        var localizationTable = localizationsList.GetLocalizationTable(languageCode);
        if (!localizationTable) {
            if (languageCode == "en-US") {
                Debug.LogError($"[{nameof(LocalizationManager)}] Default localization 'en-US' not found in LocalizationsList!");
                return;
            }

            Debug.LogWarning($"[{nameof(LocalizationManager)}] Localization '{languageCode}' not found. Falling back to en-US.");
            SetLocalization("en-US");
            return;
        }

        CurrentLocalization = localizationTable;
        OnLocalizationChanged?.Invoke();
    }

    public string GetLocalizedText(LocalizationItem item, string languageCode = null)
    {
        if (!item) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localization Item is not valid!");
            return null;
        }

        if (languageCode == null) {
            languageCode = GetCurrentLocalizationCode();
            if (languageCode == null) return item.name;
        }

        return GetText(item.name, languageCode);
    }

    public string GetText(LocalizationItem item, ILocalizable localizable)
    {
        var text = GetLocalizedText(item);
        if (text == null) return null;

        if (localizable == null) {
            Debug.LogError($"[{nameof(LocalizationManager)}] localizable is not valid for item {item.name}");
            return text;
        }

        var dict = localizable.GetLocalization();
        if (dict == null) return text;

        foreach (var key in dict.Keys.ToArray()) {
            string holder = "{" + key + "}";
            string value = dict[key];
            if (value != null) {
                text = text.Replace(holder, value);
            }
        }

        return text;
    }

    public string GetText(string key, string languageCode = null)
    {
        if (languageCode == null) {
            languageCode = GetCurrentLocalizationCode();
            if (languageCode == null) return key;
        }

        var localization = GetDeserializedLocalization(languageCode);
        if (localization == null) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Cannot get deserialized localization for language: {languageCode}");
            if (languageCode == "en-US") return key;

            return GetText(key, "en-US");
        }

        if (!localization.TryGetValue(key, out var text)) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localization {languageCode} key not found: '{key}'");

            if (languageCode != "en-US") {
                return GetText(key, "en-US");
            }
            return key;
        }

        return text;
    }

    public TMP_FontAsset GetFont(TextRole role, string languageCode = null)
    {
        if (!localizationsList) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localizations List is not valid!");
            return null;
        }

        if (languageCode == null) {
            languageCode = GetCurrentLocalizationCode();
            if (languageCode == null) return null;
        }

        var fontIndex = (int)role;

        var localizationTable = localizationsList.GetLocalizationTable(languageCode);
        if (!localizationTable) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localization Table is not valid by code {languageCode}!");
            return null;
        }

        var fonts = localizationTable.Fonts;

        if (fonts != null && fonts.Length > fontIndex) {
            return fonts[fontIndex];
        }

        return null;
    }

    public string GetLanguageNameByLocalization(string languageCode)
    {
        var localization = GetDeserializedLocalization(languageCode);
        if (localization == null) return languageCode;

        if (localization.TryGetValue("language_name", out var languageName)) {
            return languageName;
        }

        return languageCode;
    }

    private Dictionary<string, string> GetDeserializedLocalization(string languageCode)
    {
        if (!localizationsList) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Localizations List is not valid!");
            return null;
        }

        var localizationTable = localizationsList.GetLocalizationTable(languageCode);
        if (!localizationTable || localizationTable.LocalizationAsset == null) return null;

        try {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(localizationTable.LocalizationAsset.text);
        }
        catch (Exception ex) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Failed to deserialize JSON for {languageCode}: {ex.Message}");
            return null;
        }
    }

    private string GetCurrentLocalizationCode()
    {
        if (!CurrentLocalization) {
            Debug.LogError($"[{nameof(LocalizationManager)}] Current Localization Table is not valid!");
            return null;
        }

        return CurrentLocalization.LanguageCode;
    }
}