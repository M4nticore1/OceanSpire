using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationsList", menuName = "Lists/LocalizationsList")]
public class LocalizationsList : ScriptableObject
{
    private static LocalizationsList instance = null;
    public static LocalizationsList Instance
    {
        get {
            if (instance == null) {
                instance = Resources.Load<LocalizationsList>("Lists/LocalizationsList");
            }
            return instance;
        }
    }

    [SerializeField] private LocalizationTable[] localizations = null;
    public LocalizationTable[] Localizations => localizations;

    private Dictionary<SystemLanguage, LocalizationTable> localizationsDict;
    public Dictionary<SystemLanguage, LocalizationTable> LocalizationsDict
    {
        get {
            if (localizationsDict == null) {
                localizationsDict = new();

                foreach (var localization in localizations) {
                    localizationsDict.Add(localization.Language, localization);
                }
            }

            return localizationsDict;
        }
    }

    public LocalizationTable GetLocalization(SystemLanguage language)
    {
        if (!localizationsDict.TryGetValue(language, out var localization)) return null;

        return localization;
    }
}