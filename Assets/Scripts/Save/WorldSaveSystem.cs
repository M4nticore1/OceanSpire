using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class WorldSaveSystem
{
    private static string saveFileExtension = ".sav";

    public static void SaveWorld(WorldData worldData)
    {
        string worldName = worldData.WorldName;
        string folderPath = GetSaveFolderPathByName(worldName);

        Directory.CreateDirectory(folderPath);

        string filePath = GetSaveFilePathByName(worldName);
        string json = JsonConvert.SerializeObject(worldData, Formatting.Indented);

        File.WriteAllText(filePath, json);
    }

    public static WorldData GetWorldDataByName(string worldName)
    {
        string path = GetSaveFilePathByName(worldName);
        return GetSaveDataByPath(path);
    }

    public static WorldData[] GetAllSaveData()
    {
        if (!Directory.Exists(GetSavesFolderPath())) {
            Debug.Log("Save folder not found: " + GetSavesFolderPath());
            return null;
        }

        string[] filePaths = Directory.GetFiles(GetSavesFolderPath(), $"*{saveFileExtension}", SearchOption.AllDirectories);
        List<WorldData> datas = new List<WorldData>();

        foreach (string filePath in filePaths) {
            var data = GetSaveDataByPath(filePath);
            if (data == null) continue;

            datas.Add(data);
        }

        return datas.ToArray();
    }

    public static void RemoveSaveByWorldName(string worldName)
    {
        string path = GetSaveFolderPathByName(worldName);

        if (!Directory.Exists(path)) {
            Debug.Log("Save folder not found: " + path);
            return;
        }

        foreach (var file in Directory.GetFiles(path)) {
            File.Delete(file);
        }

        try {
            Directory.Delete(path, true);
            Debug.Log("The save has been deleted!");
        }
        catch (IOException ex) {
            Debug.LogError($"Couldn't delete save: {ex.Message}");
        }

    }

    public static void SaveWorldThumb(WorldData worldData)
    {
        Camera camera = Camera.main;
        int resolution = 256;

        float originalFov = camera.fieldOfView;
        camera.fieldOfView = 40;

        RenderTexture rt = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = rt;

        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        camera.Render();

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        tex.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(rt);

        camera.fieldOfView = originalFov;

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(GetSaveThumbPathByName(worldData.WorldName), bytes);
    }

    public static Texture2D GetSaveScreenshotByWorldName(string worldName)
    {
        if (!Directory.Exists(GetSavesFolderPath())) {
            Debug.LogWarning("Save folder not found: " + GetSavesFolderPath());
            return null;
        }

        string path = GetSaveThumbPathByName(worldName);
        if (!File.Exists(path)) {
            Debug.LogWarning("Save thumb not found: " + path);
            return null;
        }

        byte[] data = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (!tex.LoadImage(data)) {
            Debug.LogWarning("Failed to load image: " + path);
            return null;
        }

        return tex;
    }

    private static WorldData GetSaveDataByPath(string path)
    {
        if (!File.Exists(path)) {
            Debug.Log("Save file not found in " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        var worldData = WorldDataMigrator.GetWorldData(json);

        return worldData;
    }

    private static string GetSavesFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, "Worlds");
    }

    private static string GetSaveFolderPathByName(string worldName)
    {
        return Path.Combine(GetSavesFolderPath(), worldName);
    }

    private static string GetSaveFilePathByName(string worldName)
    {
        return Path.Combine(GetSaveFolderPathByName(worldName), worldName + saveFileExtension);
    }

    private static string GetSaveThumbPathByName(string worldName)
    {
        return Path.Combine(GetSaveFolderPathByName(worldName), worldName + ".png");
    }
}