using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldSaveSystem
{
    private static string saveFileExtension = ".sav";

    public static void SaveData(PlayerController playerController)
    {
        string worldName = WorldSaveManager.Instance.saveWorldName;
        string folderPath = GetSaveFolderPathByName(worldName);

        Directory.CreateDirectory(folderPath);

        string filePath = GetSaveFilePathByName(worldName);

        if (File.Exists(filePath)) {
            JsonConvert.SerializeObject(filePath);
        }

        SaveScreenshotByWorldName(worldName);
    }

    public static WorldData GetSaveDataByWorldName(string worldName)
    {
        string path = GetSaveFilePathByName(worldName);
        return GetSaveDataByPath(path);
    }

    private static WorldData GetSaveDataByPath(string path)
    {
        if (!File.Exists(path)) {
            Debug.Log("Save file not found in " + path);
            return null;
        }

        try {
            string json = File.ReadAllText(path);
            WorldData data = JsonConvert.DeserializeObject<WorldData>(json);
            return data;
        }
        catch (Exception e) {
            Debug.LogError($"Failed to load save {path}\n{e}");
            return null;
        }
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
            WorldData data = GetSaveDataByPath(filePath);

            if (data != null) {
                datas.Add(data);
            }
            Debug.Log("get");
        }

        return datas.ToArray();
    }

    public static void RemoveSaveByWorldName(string worldName)
    {
        string path = GetSaveFolderPathByName(worldName);
        if (!Directory.Exists(path)) {
            Debug.Log("Файл сохранения не найден.");
            return;
        }

        foreach (var file in Directory.GetFiles(path)) {
            File.Delete(file);
        }
        Directory.Delete(path);
        Debug.Log("Сохранение удалено!");
    }

    private static string GetSavesFolderPath()
    {
        string endPath = Path.Combine("Banzai Games", "Ocean Spire", "Saves");
        return Path.Combine(Application.persistentDataPath, "Saves");
        //if (Application.isMobilePlatform || Application.isConsolePlatform) {
        //    return Path.Combine(Application.persistentDataPath, endPath);
        //}

        //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), endPath);
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

    public static void SaveScreenshotByWorldName(string worldName)
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
        File.WriteAllBytes(GetSaveThumbPathByName(worldName), bytes);
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
}
