using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LocalizationData
{
    public Dictionary<string, string> content;
    public string[] male_first_names;
    public string[] male_last_names;
    public string[] female_first_names;
    public string[] female_last_names;
}

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (instance == null)
                instance = new LocalizationManager();
            return instance;
        }
    }

    public List<LocalizationData> localizations { get; private set; } = new List<LocalizationData>();
    public LocalizationData currentLocalization = null;

    public bool isInitialized { get; private set; } = false;
    public event System.Action OnLocalizationChanged;

    private LocalizationManager() { }

    public async Task InitAsync()
    {
        localizations = await LocalizationSystem.GetLocalizationsAsync();
        Debug.Log("Loaded " + localizations.Count + " localizations");
        isInitialized = true;
    }

    public string GetLocalizationText(string key)
    {
        if (!isInitialized) return key;

        if (currentLocalization != null) {
            if (currentLocalization.content.ContainsKey(key)) {
                return currentLocalization.content[key];
            }
            else {
                Debug.LogWarning($"localizations[currentLocalizationIndex] has no {key} key");
                return "";
            }
        }
        else
            return key;
    }

    public string GetFirstName(bool isMale, int index)
    {
        if (isMale) {
            return GetName(isMale, index, currentLocalization.male_first_names);
        }
        else {
            return GetName(isMale, index, currentLocalization.female_first_names);
        }
    }

    public string GetLastName(bool isMale, int index)
    {
        if (isMale) {
            return GetName(isMale, index, currentLocalization.male_last_names);
        }
        else {
            return GetName(isMale, index, currentLocalization.female_last_names);
        }
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

    public void SetLocalization(string languageKey)
    {
        for (int i = 0; i < localizations.Count; i++) {
            if (localizations[i].content["language.code"] == languageKey) {
                currentLocalization = localizations[i];
                break;
            }
        }
        OnLocalizationChanged?.Invoke();
    }
}
