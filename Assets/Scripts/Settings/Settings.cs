using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    public static string currentLanguageKey = "en";

    public static void ChangeLanguage(string languageKey)
    {
        LocalizationSystem.SetLocalization(languageKey);
    }
}
