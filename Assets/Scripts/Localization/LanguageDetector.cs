using System;
using System.Globalization;
using UnityEngine;

public static class LanguageDetector
{
    public static string GetSystemGameLanguage()
    {
        string languageCode = "en";
        string countryCode = "US";

#if UNITY_ANDROID && !UNITY_EDITOR
        try 
        {
            using (var localeClass = new AndroidJavaClass("java.util.Locale"))
            using (var defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault"))
            {
                languageCode = defaultLocale.Call<string>("getLanguage").ToLower();
                countryCode = defaultLocale.Call<string>("getCountry").ToUpper();
            }
        }
        catch (Exception e) 
        {
            Debug.LogWarning($"[LanguageDetector] Не удалось получить локаль Android через Java: {e.Message}");
            // Если Java упала, опрашиваем .NET прямо тут
            GetDotNetCulture(out languageCode, out countryCode);
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // На iOS сразу берем системный .NET CultureInfo
        GetDotNetCulture(out languageCode, out countryCode);
#else
        // В Редакторе / на ПК тоже берем текущую культуру ОС
        GetDotNetCulture(out languageCode, out countryCode);
#endif

        // Передаем то, что определили, на финальную сборку
        return ParseCultureInfo(languageCode, countryCode);
    }

    private static void GetDotNetCulture(out string lang, out string country)
    {
        lang = "en";
        country = "US";

        string cultureName = CultureInfo.CurrentCulture.Name;
        if (!string.IsNullOrEmpty(cultureName)) {
            string[] parts = cultureName.Split('-');
            if (parts.Length > 0) lang = parts[0].ToLower();
            if (parts.Length > 1) country = parts[1].ToUpper();
        }
    }

    private static string ParseCultureInfo(string languageCode, string countryCode)
    {
        if (languageCode == string.Empty || countryCode == string.Empty) {
            languageCode = "en";
            countryCode = "US";
        }

        return $"{languageCode}-{countryCode}";
    }
}