using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    private static string saveFileExtension = ".sav";

    public static void SaveData(PlayerController playerController)
    {
        SaveData playerData = new SaveData(playerController);
        BinaryFormatter formatter = new BinaryFormatter();

        string worldName = SaveManager.Instance.saveWorldName;
        string folderPath = GetSaveFolderPathByName(worldName);

        Directory.CreateDirectory(folderPath);

        string filePath = GetSaveFilePathByName(worldName);

        using (FileStream stream = new FileStream(filePath, FileMode.Create)) {
            formatter.Serialize(stream, playerData);
        }

        SaveScreenshotByWorldName(worldName);
    }

    public static SaveData GetSaveDataByWorldName(string worldName)
    {
        string path = GetSaveFilePathByName(worldName);
        return GetSaveDataByPath(path);
    }

    private static SaveData GetSaveDataByPath(string path)
    {
        Debug.Log(path);
        if (File.Exists(path)) {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            if (stream.Length > 0) {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();

                Debug.Log(path);
                return data;
            }
            else {
                Debug.Log("Stream is empty");
                return null;
            }
        }
        else {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }

    public static SaveData[] GetAllSaveData()
    {
        if (!Directory.Exists(GetSavesFolderPath())) {
            Debug.Log("Save folder not found: " + GetSavesFolderPath());
            return Array.Empty<SaveData>();
        }

        string[] filePaths = Directory.GetFiles(GetSavesFolderPath(), $"*{saveFileExtension}", SearchOption.AllDirectories);
        List<SaveData> datas = new List<SaveData>();
        foreach (string filePath in filePaths) {
            SaveData data = GetSaveDataByPath(filePath);
            if (data != null)
                datas.Add(data);
        }
        return datas.ToArray();
    }

    public static void RemoveSave(string worldName)
    {
        string path = GetSaveFilePathByName(worldName);
        if (!File.Exists(path)) {
            Debug.Log("Файл сохранения не найден.");
            return;
        }

        File.Delete(path);
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
