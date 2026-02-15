using System.IO;
using UnityEngine;

public static class SettingsSaveSystem
{
    private static string fileName = "PlayerSettings.json";

    public static SettingsData GetData()
    {
        string filePath = GetFile();
        if (!File.Exists(filePath)) return null;

        SettingsData data = null;
        data = JsonUtility.FromJson<SettingsData>(filePath);
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
