using UnityEngine;

public static class LanguageNameTranslater
{
    public static string GetNativeLanguageName(SystemLanguage language)
    {
        switch (language) {
            case SystemLanguage.English:
                return "English";
            case SystemLanguage.French:
                return "Français";
            case SystemLanguage.German:
                return "Deutsch";
            case SystemLanguage.Spanish:
                return "Español";
            case SystemLanguage.Russian:
                return "Русский";
            default:
                return language.ToString();
        }
    }
}