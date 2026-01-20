using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance => instance ??= new LocalizationManager();

    public List<Dictionary<string, string>> localizations { get; private set; } = new List<Dictionary<string, string>>();
    private int currentLocalizationIndex = 0;
    public bool isInitialized { get; private set; } = false;
    public event System.Action OnLocalizationChanged;

    public async Task InitializeAsync()
    {
        localizations = await LocalizationSystem.LoadLocalizationsAsync();
        Debug.Log("Loaded " + localizations.Count + " localizations");
        isInitialized = true;
    }

    public string GetLocalizationText(string key)
    {
        if (!isInitialized) return key;

        if (localizations.Count > currentLocalizationIndex && localizations[currentLocalizationIndex] != null) {
            if (localizations[currentLocalizationIndex].ContainsKey(key))
                return localizations[currentLocalizationIndex][key];
            else {
                Debug.LogError($"localizations[currentLocalizationIndex] has no {key} key");
                return "";
            }
        }
        else
            return key;
    }

    public void SetLocalization(string languageKey)
    {
        for (int i = 0; i < localizations.Count; i++) {
            if (localizations[i]["language.code"] == languageKey) {
                currentLocalizationIndex = i;
                break;
            }
        }
        OnLocalizationChanged?.Invoke();
    }
}
