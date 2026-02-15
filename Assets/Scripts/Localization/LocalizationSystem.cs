//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//using Newtonsoft.Json;
//using System.Threading.Tasks;
//using UnityEngine.Networking;

//public static class LocalizationSystem
//{
//    private static readonly string LocalizationFolderName = "Localization";
//    private static readonly string LocalizationsFolderPath = Path.Combine(Application.streamingAssetsPath, LocalizationFolderName);
//    private static readonly string LocalizationListPath = Path.Combine(LocalizationsFolderPath, "localizations_list.json");
//    private static string ContentFilePath(string langKey) => Path.Combine(LocalizationsFolderPath, $"{langKey}.json");
//    private static string NamesFilePath(string langKey) => Path.Combine(LocalizationsFolderPath, langKey, "names.json");

//    public static async Task<List<LocalizationData>> GetLocalizationsAsync()
//    {
//        List<LocalizationData> localizationDict = new List<LocalizationData>();
//        string[] localizationKeys = new string[0];

//        if (Application.platform == RuntimePlatform.Android) {
//            // Get language keys from localization list
//            try {
//                string localizationListContent = await GetTextFromStreamingAssetsAsync(LocalizationListPath);
//                localizationKeys = JsonConvert.DeserializeObject<string[]>(localizationListContent);
//            }
//            catch (System.Exception e) {
//                Debug.LogError($"Failed to load localization list: {e.Message}");
//                return localizationDict;
//            }

//            // Get localizations from keys
//            foreach (string key in localizationKeys) {
//                try {
//                    string content = await GetTextFromStreamingAssetsAsync(ContentFilePath(key));
//                    var dict = JsonConvert.DeserializeObject<LocalizationData>(content);
//                    localizationDict.Add(dict);
//                }
//                catch (System.Exception e) {
//                    Debug.LogError($"Failed to load localization from {ContentFilePath(key)}: {e.Message}");
//                }
//            }
//        }
//        else {
//            // Get language keys from localization list
//            if (File.Exists(LocalizationListPath)) {
//                string localizationListContent = File.ReadAllText(LocalizationListPath);
//                localizationKeys = JsonConvert.DeserializeObject<string[]>(localizationListContent);
//            }
//            else {
//                Debug.LogError(LocalizationListPath + " is not found");
//                return localizationDict;
//            }

//            // Get localizations from keys
//            foreach (string key in localizationKeys) {
//                string filePath = ContentFilePath(key);
//                if (!File.Exists(filePath)) continue;

//                try {
//                    string jsonContent = File.ReadAllText(filePath);
//                    var dict = JsonConvert.DeserializeObject<LocalizationData>(jsonContent);
//                    localizationDict.Add(dict);
//                }
//                catch (System.Exception e) {
//                    Debug.LogError($"Failed to load localization from {filePath}: {e.Message}");
//                }
//            }
//        }

//        return localizationDict;
//    }

//    private static async Task<string> GetTextFromStreamingAssetsAsync(string path)
//    {
//        using var request = UnityWebRequest.Get(path);
//        var operation = request.SendWebRequest();

//        while (!operation.isDone)
//            await Task.Yield();

//        if (request.result != UnityWebRequest.Result.Success)
//            throw new System.Exception(request.error);

//        return request.downloadHandler.text;
//    }
//}