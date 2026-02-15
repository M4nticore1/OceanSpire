using System.Collections.Generic;
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

    private Dictionary<SystemLanguage, LocalizationTable> localizations = new Dictionary<SystemLanguage, LocalizationTable>();
    public LocalizationTable currentLocalization { get; private set; } = null;

    public bool isInited { get; private set; } = false;
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
            SystemLanguage systemLanguage = Application.systemLanguage;

            if (localizations.ContainsKey(systemLanguage)) {
                SetLocalization(Application.systemLanguage);
            }
            else {
                SetLocalization(SystemLanguage.English);
            }
        }

        isInited = true;
    }

    public LocalizationEntry GetLocalizationEntry(LocalizationItem item)
    {
        if (!item) {
            Debug.LogError("item is not valid.");
            return null;
        }

        if (!currentLocalization) {
            Debug.LogError("currentLocalization is not valid.");
            return null;
        }

        if (!currentLocalization.itemsDict.ContainsKey(item)) {
            Debug.LogError($"currentLocalizationIndex has no '{item.name}' key");
            return null;
        }

        return currentLocalization.itemsDict[item];
    }

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

    public void SetLocalization(SystemLanguage language)
    {
        currentLocalization = localizations[language];
        OnLocalizationChanged?.Invoke();
    }
}
