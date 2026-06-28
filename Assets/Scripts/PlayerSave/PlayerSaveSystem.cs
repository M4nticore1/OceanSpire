using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class PlayerSaveSystem
{
    private static string fileName = "Player.sav";

    public static void SaveData(PlayerData playerSettingsData)
    {
        string folderPath = GetFolder();
        if (string.IsNullOrEmpty(folderPath)) {
            Debug.LogError("FolderPath is null or empty!");
            return;
        }

        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, fileName);
        string json = JsonConvert.SerializeObject(playerSettingsData, Formatting.Indented);

        File.WriteAllText(filePath, json);
    }

    public static PlayerData GetData()
    {
        string filePath = GetFile();
        if (!File.Exists(filePath)) return null;

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrEmpty(json)) {
            Debug.LogError("Save file is empty");
            return null;
        }

        var data = JsonConvert.DeserializeObject<PlayerData>(json);
        return data;
    }

    private static string GetFolder()
    {
        return Application.persistentDataPath;
    }

    private static string GetFile()
    {
        return Path.Combine(GetFolder(), fileName);
    }
}