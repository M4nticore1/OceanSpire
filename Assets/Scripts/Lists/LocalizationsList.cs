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

    private Dictionary<string, LocalizationTable> localizationsDict;
    public Dictionary<string, LocalizationTable> LocalizationsDict
    {
        get {
            if (localizationsDict == null) {
                localizationsDict = new();

                foreach (var localization in localizations) {
                    localizationsDict.Add(localization.LanguageCode, localization);
                }
            }

            return localizationsDict;
        }
    }

    public LocalizationTable GetLocalization(string languageCode)
    {
        if (languageCode == null) return null;
        if (languageCode == string.Empty) return null;
        if (!LocalizationsDict.TryGetValue(languageCode, out var localization)) return null;

        return localization;
    }
}