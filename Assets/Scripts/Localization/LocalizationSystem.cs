using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LocalizationSystem
{
    [UnityEditor.MenuItem("Tools/Generate Localization File")]
    public static void GenerateLocalizationFile()
    {
        List<string> keys = new List<string>
        {
            "language.name",
            "menu.load",
            "menu.remove",
            "menu.exit",
            "menu.close",
            "menu.level",
            "building.livingRooms.name",
            "building.livingRooms.description",
            "building.woodGenerator.name",
            "building.woodGenerator.description",
            "building.characteristic.storageCapacity",
            "item.wood",
            "item.stone",
            "item.metal",
            "item.plastic",
        };

        string dir = Path.Combine(Application.streamingAssetsPath, "Localization");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "localization.json");

        // „итаем существующий JSON
        Dictionary<string, string> existingDict = File.Exists(path) ? JsonToDictionary(File.ReadAllText(path)) : new Dictionary<string, string>();

        // —оздаем новый словарь дл€ сохранени€ пор€дка из списка
        var orderedDict = new Dictionary<string, string>();

        foreach (var key in keys)
        {
            if (existingDict.ContainsKey(key))
                orderedDict[key] = existingDict[key]; // старое значение сохран€ем
            else
                orderedDict[key] = ""; // новое значение пустое
        }

        // —охран€ем JSON
        File.WriteAllText(path, DictionaryToJson(orderedDict));
        Debug.Log("JSON file updated at: " + path);
    }

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
}