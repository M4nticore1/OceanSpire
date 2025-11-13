using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class LocalizationSystem
{
    private static string localizationFolder = "Localization";

    public static bool areLocalizationsInitialized { get; private set; } = false;
    public static List<Dictionary<string, string>> localizations { get; private set; } = new List<Dictionary<string, string>>();
    private static int currentLocalizationIndex = 0;

    public static event System.Action OnLocalizationChanged;

    private static Dictionary<string, string> originalLocalization = new Dictionary<string, string>
    {
        { "language.name", "English" },
        { "language.code", "en" },
        { "common.building", "Building" },
        { "common.construction", "Construction" },
        { "common.storage", "Storage" },
        { "common.level", "Level" },
        { "menu.load", "Load"},
        { "menu.remove", "Remove" },
        { "menu.exit", "Exit" },
        { "menu.close", "Close" },
        { "building.livingRooms.name", "Living Rooms" },
        { "building.livingRooms.description", "" },
        { "building.woodGenerator.name", "Wood Generator" },
        { "building.woodGenerator.description", "" },
        { "building.stats.storageCapacity", "Capacity" },
        { "stats.population", "Population" },
        { "stats.food", "Food" },
        { "stats.water", "Water" },
        { "stats.electricity", "Electricity" },
        { "item.stone", "Stone" },
        { "item.metal", "Metal" },
        { "item.plastic", "Plastic" },

    };

#if UNITY_EDITOR
    [MenuItem("Tools/Generate Localization File")]
    public static void GenerateLocalizationFile()
    {
        string dir = Path.Combine(Application.streamingAssetsPath, localizationFolder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "localization.json");

        string json = JsonConvert.SerializeObject(originalLocalization, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("JSON file updated at: " + path);
    }

    [MenuItem("Tools/Update All Localizations File")]
    public static void UpdateAllLocalizationFiles()
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, localizationFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string[] files = Directory.GetFiles(folderPath, "*.json");

        for (int i = 0; i < files.Length; i++)
        {
            var jsonText = File.ReadAllText(files[i]);
            var localizationDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
            if (localizationDict.ContainsKey("language.code"))
            {
                string jsonName = localizationDict["language.code"] + ".json";
                string jsonPath = Path.Combine(folderPath, jsonName);

                Dictionary<string, string> existingDict = File.Exists(jsonPath) ? JsonToDictionary(File.ReadAllText(jsonPath)) : new Dictionary<string, string>();

                var updatedDict = new Dictionary<string, string>();

                foreach (var kvp in originalLocalization)
                {
                    if (existingDict.ContainsKey(kvp.Key))
                        updatedDict[kvp.Key] = existingDict[kvp.Key];
                    else
                        updatedDict[kvp.Key] = kvp.Value;
                }

                File.WriteAllText(jsonPath, DictionaryToJson(updatedDict));
                Debug.Log("JSON file updated at: " + jsonPath);
            }
        }
    }
#endif

    static string DictionaryToJson(Dictionary<string, string> dict)
    {
        List<string> lines = new List<string> { "{" };
        int i = 0;
        foreach (var kvp in dict)
        {
            string valueEscaped = kvp.Value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            string comma = (i < dict.Count - 1) ? "," : "";
            lines.Add($"  \"{kvp.Key}\": \"{valueEscaped}\"{comma}");
            i++;
        }
        lines.Add("}");
        return string.Join("\n", lines);
    }

    static Dictionary<string, string> JsonToDictionary(string json)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(json)) return dict;

        json = json.Trim().TrimStart('{').TrimEnd('}');
        var entries = json.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var kv = entry.Split(new[] { ':' }, 2);
            if (kv.Length == 2)
            {
                string key = kv[0].Trim().Trim('"');
                string value = kv[1].Trim().Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");
                dict[key] = value;
            }
        }
        return dict;
    }

    public static void LoadLocalizations()
    {
        if (areLocalizationsInitialized)
            return;

        string folderPath = Path.Combine(Application.streamingAssetsPath, localizationFolder);

        string[] files;

#if UNITY_ANDROID && !UNITY_EDITOR
        // На Android Directory.GetFiles не работает, поэтому нужно использовать прямой список файлов
        files = new string[] { "en.json", "ru.json" }; // укажи свои файлы
#else
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        files = Directory.GetFiles(folderPath, "*.json");
#endif

        foreach (var fileName in files)
        {
            string json;

#if UNITY_ANDROID && !UNITY_EDITOR
            string path = Path.Combine(Application.streamingAssetsPath, localizationFolder, fileName);
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone) { } // ждем завершения запроса
            json = www.downloadHandler.text;
#else
            json = File.ReadAllText(fileName);
#endif

            var localizationDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            localizations.Add(localizationDict);
        }

        areLocalizationsInitialized = true;
    }

    public static string GetLocalizationText(string key)
    {
        if (localizations.Count > currentLocalizationIndex && localizations[currentLocalizationIndex] != null)
            return localizations[currentLocalizationIndex][key];
        else
            return originalLocalization[key];
    }

    public static void SetLocalization(string languageKey)
    {
        for (int i = 0; i < localizations.Count; i++)
        {
            if (localizations[i]["language.code"] == languageKey)
            {
                currentLocalizationIndex = i;
                break;
            }
        }

        OnLocalizationChanged?.Invoke();
    }
}