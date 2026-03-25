using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

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

    public Dictionary<SystemLanguage, LocalizationTable> localizations { get; private set; } = new Dictionary<SystemLanguage, LocalizationTable>();
    public LocalizationTable currentLocalization { get; private set; }
    private Dictionary<string, string> deserializedLocalization;

    private bool isInited = false;
    public event System.Action OnLocalizationChanged;

    private LocalizationManager() { }

    public void Init(SettingsData data)
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
            SystemLanguage systemLanguage = SystemLanguage.Russian /*Application.systemLanguage*/;

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
        string text = deserializedLocalization[key];
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

    //public LocalizationEntry GetLocalizationEntry(LocalizationItem item)
    //{
    //    if (!item) {
    //        Debug.LogError("item is not valid.");
    //        return null;
    //    }

    //    if (!currentLocalization) {
    //        Debug.LogError("currentLocalization is not valid.");
    //        return null;
    //    }

    //    if (!currentLocalization.itemsDict.ContainsKey(item)) {
    //        Debug.LogError($"currentLocalizationIndex has no '{item.name}' key");
    //        return null;
    //    }

    //    return currentLocalization.itemsDict[item];
    //}

    public string GetFirstName(bool isMale, int index)
    {
        if (isMale) {
            //return GetName(isMale, index, currentLocalization.male_first_names);
        }
        else {
            //return GetName(isMale, index, currentLocalization.female_first_names);
        }
        return "";
    }

    public string GetLastName(bool isMale, int index)
    {
        if (isMale) {
            //return GetName(isMale, index, currentLocalization.male_last_names);
        }
        else {
            //return GetName(isMale, index, currentLocalization.female_last_names);
        }
        return "";
    }

    private string GetName(bool isMale, int index, string[] names)
    {
        if (names == null) {
            Debug.LogError("Array is not valid.");
            return "";
        }

        int maxIndex = names.Length;
        int finalIndex = index % maxIndex;

        return names[index];
    }
}
