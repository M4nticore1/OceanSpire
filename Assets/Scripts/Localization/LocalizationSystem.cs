using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class LocalizationSystem
{
    private static readonly string LocalizationFolderName = "Localization";
    private static readonly string LocalizationsFolderPath = Path.Combine(Application.streamingAssetsPath, LocalizationFolderName);
    private static readonly string LocalizationListPath = Path.Combine(LocalizationsFolderPath, "localization_list.json");
    private static string LanguageFilePath(string lang) => Path.Combine(LocalizationsFolderPath, $"{lang}.json");

    //public readonly static Dictionary<string, string> originalLocalization = new Dictionary<string, string>
    //{
    //    { "language.name", "English" },
    //    { "language.code", "en" },
    //    { "common.building", "Building" },
    //    { "common.construction", "Construction" },
    //    { "common.storage", "Storage" },
    //    { "common.level", "Level" },
    //    { "menu.load", "Load"},
    //    { "menu.remove", "Remove" },
    //    { "menu.exit", "Exit" },
    //    { "menu.close", "Close" },
    //    { "building.livingRooms.name", "Living Rooms" },
    //    { "building.livingRooms.description", "" },
    //    { "building.woodGenerator.name", "Wood Generator" },
    //    { "building.woodGenerator.description", "" },
    //    { "building.stats.storageCapacity", "Capacity" },
    //    { "stats.population", "Population" },
    //    { "stats.food", "Food" },
    //    { "stats.water", "Water" },
    //    { "stats.electricity", "Electricity" },
    //    { "item.stone", "Stone" },
    //    { "item.metal", "Metal" },
    //    { "item.plastic", "Plastic" },

    //};

//#if UNITY_EDITOR
//    [MenuItem("Tools/Generate Localization File")]
//    public static void GenerateLocalizationFile()
//    {
//        string dir = Path.Combine(Application.streamingAssetsPath, LocalizationFolderName);
//        if (!Directory.Exists(dir))
//            Directory.CreateDirectory(dir);
//        string path = Path.Combine(dir, "localization.json");

//        string json = JsonConvert.SerializeObject(originalLocalization, Formatting.Indented);
//        File.WriteAllText(path, json);
//        Debug.Log("JSON file updated at: " + path);
//    }

//    [MenuItem("Tools/Update All Localizations File")]
//    public static void UpdateAllLocalizationFiles()
//    {
//        string folderPath = Path.Combine(Application.streamingAssetsPath, LocalizationFolderName);
//        if (!Directory.Exists(folderPath))
//            Directory.CreateDirectory(folderPath);

//        string[] files = Directory.GetFiles(folderPath, "*.json");

//        for (int i = 0; i < files.Length; i++) {
//            var jsonText = File.ReadAllText(files[i]);
//            var localizationDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
//            if (localizationDict.ContainsKey("language.code")) {
//                string jsonName = localizationDict["language.code"] + ".json";
//                string jsonPath = Path.Combine(folderPath, jsonName);

//                Dictionary<string, string> existingDict = File.Exists(jsonPath) ? JsonToDictionary(File.ReadAllText(jsonPath)) : new Dictionary<string, string>();

//                var updatedDict = new Dictionary<string, string>();

//                foreach (var kvp in originalLocalization) {
//                    if (existingDict.ContainsKey(kvp.Key))
//                        updatedDict[kvp.Key] = existingDict[kvp.Key];
//                    else
//                        updatedDict[kvp.Key] = kvp.Value;
//                }

//                File.WriteAllText(jsonPath, DictionaryToJson(updatedDict));
//                Debug.Log("JSON file updated at: " + jsonPath);
//            }
//        }
//    }
//#endif

//    static string DictionaryToJson(Dictionary<string, string> dict)
//    {
//        List<string> lines = new List<string> { "{" };
//        int i = 0;
//        foreach (var kvp in dict)
//        {
//            string valueEscaped = kvp.Value.Replace("\\", "\\\\").Replace("\"", "\\\"");
//            string comma = (i < dict.Count - 1) ? "," : "";
//            lines.Add($"  \"{kvp.Key}\": \"{valueEscaped}\"{comma}");
//            i++;
//        }
//        lines.Add("}");
//        return string.Join("\n", lines);
//    }

//    static Dictionary<string, string> JsonToDictionary(string json)
//    {
//        Dictionary<string, string> dict = new Dictionary<string, string>();
//        if (string.IsNullOrWhiteSpace(json)) return dict;

//        json = json.Trim().TrimStart('{').TrimEnd('}');
//        var entries = json.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
//        foreach (var entry in entries)
//        {
//            var kv = entry.Split(new[] { ':' }, 2);
//            if (kv.Length == 2)
//            {
//                string key = kv[0].Trim().Trim('"');
//                string value = kv[1].Trim().Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");
//                dict[key] = value;
//            }
//        }
//        return dict;
//    }

    public static async Task<List<Dictionary<string, string>>> LoadLocalizationsAsync()
    {
        List<Dictionary<string, string>> localizationDict = new List<Dictionary<string, string>>();
        string[] localizationKeys = new string[0];

        if (Application.platform == RuntimePlatform.Android) {
            // Get language keys from localization list
            try {
                string localizationListContent = await GetTextFromStreamingAssetsAsync(LocalizationListPath);
                localizationKeys = JsonConvert.DeserializeObject<string[]>(localizationListContent);
            }
            catch (System.Exception e) {
                Debug.LogError($"Failed to load localization list: {e.Message}");
                return localizationDict;
            }

            // Get localizations from keys
            foreach (string key in localizationKeys) {
                try {
                    string content = await GetTextFromStreamingAssetsAsync(LanguageFilePath(key));
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                    localizationDict.Add(dict);
                }
                catch (System.Exception e) {
                    Debug.LogError($"Failed to load localization from {LanguageFilePath(key)}: {e.Message}");
                }
            }
        }
        else {
            // Get language keys from localization list
            if (File.Exists(LocalizationListPath)) {
                string localizationListContent = File.ReadAllText(LocalizationListPath);
                localizationKeys = JsonConvert.DeserializeObject<string[]>(localizationListContent);
            }
            else {
                Debug.LogError(LocalizationListPath + " is not found");
                return localizationDict;
            }

            // Get localizations from keys
            foreach (string key in localizationKeys) {
                string filePath = LanguageFilePath(key);
                if (!File.Exists(filePath)) continue;

                try {
                    string jsonContent = File.ReadAllText(filePath);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
                    localizationDict.Add(dict);
                }
                catch (System.Exception e) {
                    Debug.LogError($"Failed to load localization from {filePath}: {e.Message}");
                }
            }
        }

        return localizationDict;
    }

    private static async Task<string> GetTextFromStreamingAssetsAsync(string path)
    {
        using var request = UnityWebRequest.Get(path);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
            throw new System.Exception(request.error);

        return request.downloadHandler.text;
    }
}