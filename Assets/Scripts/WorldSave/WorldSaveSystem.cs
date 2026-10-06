using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class WorldSaveSystem
{
    private static string saveFileExtension = ".sav";

    public static event Action<WorldData> OnWorldSaveCreated;
    public static event Action<WorldData> OnWorldSaveDeleted;
    public static event Action<WorldData> OnWorldSaveRenamed;

    public static void SaveWorld(WorldData worldData)
    {
        if (worldData == null) {
            Debug.Log($"[{nameof(WorldSaveSystem)}] WorldData is not valid!");
            return;
        }

        var worldName = worldData.WorldName;
        if (worldName == null) {
            Debug.LogError("WorldName is not valid!");
            worldData.WorldName = $"World_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            worldName = worldData.WorldName;
        }

        var folderPathName = GetSaveFolderPathByName(worldName);
        if (folderPathName == null) {
            Debug.LogError($"SaveFolderPath is not valid by name {worldName}!");
            return;
        }

        Directory.CreateDirectory(folderPathName);

        var filePath = GetSaveFilePathByName(worldName);
        if (filePath == null) {
            Debug.LogError($"SaveFilePath is not valid by name {worldName}!");
            return;
        }

        var json = JsonConvert.SerializeObject(worldData, Formatting.None);
        if (json == null) {
            Debug.LogError("SerializedObject is not valid!");
            return;
        }

        File.WriteAllTextAsync(filePath, json);

        OnWorldSaveCreated?.Invoke(worldData);
    }

    public static void DeleteSaveByWorldName(string worldName)
    {
        var isRootWorld = string.IsNullOrWhiteSpace(worldName);

        var worldData = isRootWorld ? null : GetWorldDataByName(worldName);
        if (!isRootWorld && worldData == null)
            return;

        var path = isRootWorld ? GetSavesFolderPath() : GetSaveFolderPathByName(worldName);
        var rootSavesPath = Path.GetFullPath(GetSavesFolderPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var targetWorldPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (string.Equals(rootSavesPath, targetWorldPath, System.StringComparison.OrdinalIgnoreCase) && !isRootWorld) {
            Debug.LogError($"[{nameof(WorldSaveSystem)}] CRITICAL ERROR: Attempt to delete the root saves folder was blocked by path {targetWorldPath}!");
            return;
        }

        if (!Directory.Exists(targetWorldPath)) {
            Debug.LogWarning($"[{nameof(WorldSaveSystem)}] World folder not found: " + targetWorldPath);
            return;
        }

        try {
            if (isRootWorld) {
                var files = Directory.GetFiles(targetWorldPath, "*.sav", SearchOption.TopDirectoryOnly);
                foreach (var file in files) {
                    File.Delete(file);
                }
                Debug.Log($"[{nameof(WorldSaveSystem)}] Root saves folder cleaned: deleted {files.Length} .sav files.");
            }
            else {
                Directory.Delete(targetWorldPath, true);
                Debug.Log($"[{nameof(WorldSaveSystem)}] World folder successfully deleted: {worldName}");
            }

            OnWorldSaveDeleted?.Invoke(worldData);
        }
        catch (System.Exception ex) {
            Debug.LogError($"[{nameof(WorldSaveSystem)}] Failed to process path '{targetWorldPath}': {ex.Message}");
        }
    }

    public static void RenameWorld(string oldWorldName, string newWorldName)
    {
        if (string.IsNullOrWhiteSpace(newWorldName)) {
            Debug.LogError($"[{nameof(WorldSaveSystem)}] New world name is invalid!");
            return;
        }

        var oldFolderPath = GetSaveFolderPathByName(oldWorldName);
        var newFolderPath = GetSaveFolderPathByName(newWorldName);

        try {
            if (!Directory.Exists(oldFolderPath)) {
                Debug.LogWarning($"[{nameof(WorldSaveSystem)}] Old world folder not found. Creating new folder: {newFolderPath}");
                Directory.CreateDirectory(newFolderPath);
                return;
            }

            if (oldWorldName == newWorldName)
                return;

            if (Directory.Exists(newFolderPath)) {
                Directory.Delete(newFolderPath, true);
            }

            Directory.Move(oldFolderPath, newFolderPath);

            var files = Directory.GetFiles(newFolderPath, "*.*", SearchOption.AllDirectories);
            foreach (var filePath in files) {
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(fileName);

                string newFileName;
                if (fileName.EndsWith(saveFileExtension)) {
                    newFileName = newWorldName + saveFileExtension;
                }
                else {
                    newFileName = string.IsNullOrWhiteSpace(oldWorldName) ? newWorldName + extension : fileName.Replace(oldWorldName, newWorldName);
                }

                var newFilePath = Path.Combine(newFolderPath, newFileName);

                if (filePath != newFilePath) {
                    if (File.Exists(newFilePath)) {
                        File.Delete(newFilePath);
                    }
                    File.Move(filePath, newFilePath);
                }
            }

            var newFilePathFinal = Path.Combine(newFolderPath, newWorldName + saveFileExtension);
            if (File.Exists(newFilePathFinal)) {
                var worldData = GetSaveDataByPath(newFilePathFinal);
                if (worldData != null) {
                    worldData.WorldName = newWorldName;

                    var json = JsonConvert.SerializeObject(worldData, Formatting.None);
                    File.WriteAllText(newFilePathFinal, json);
                }

                OnWorldSaveRenamed?.Invoke(worldData);
            }

            Debug.Log($"[{nameof(WorldSaveSystem)}] World successfully renamed from '{oldWorldName}' to '{newWorldName}'");
        }
        catch (System.Exception ex) {
            Debug.LogError($"[{nameof(WorldSaveSystem)}] Failed to rename world from '{oldWorldName}' to '{newWorldName}': {ex.Message}");
        }
    }

    public static void SaveWorldThumb(MonoBehaviour runner, string worldName)
    {
        if (string.IsNullOrWhiteSpace(worldName)) {
            Debug.LogError($"[{nameof(WorldSaveSystem)}] World name is invalid for thumbnail!");
            return;
        }

        runner.StartCoroutine(SaveThumbRoutine(worldName));
    }

    public static WorldData GetWorldDataByName(string worldName)
    {
        string path = GetSaveFilePathByName(worldName);
        return GetSaveDataByPath(path);
    }

    public static List<WorldData> GetAllSaveData()
    {
        if (!Directory.Exists(GetSavesFolderPath())) {
            Debug.Log("Save folder not found: " + GetSavesFolderPath());
            return null;
        }

        var filePaths = Directory.GetFiles(GetSavesFolderPath(), $"*{saveFileExtension}", SearchOption.AllDirectories);
        var datas = new List<WorldData>();

        foreach (string filePath in filePaths) {
            var data = GetSaveDataByPath(filePath);
            if (data == null) continue;

            datas.Add(data);
        }

        return datas;
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

    private static IEnumerator SaveThumbRoutine(string worldName)
    {
        yield return new WaitForEndOfFrame();

        var camera = Camera.main;
        if (camera == null) {
            Debug.LogWarning($"[{nameof(WorldSaveSystem)}] Main Camera not found for thumbnail!");
            yield break;
        }

        var resolution = 256;
        var originalFov = camera.fieldOfView;
        camera.fieldOfView = 40;

        var rt = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = rt;

        var tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        camera.Render();

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        tex.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(rt);

        camera.fieldOfView = originalFov;

        var thumbPath = GetSaveThumbPathByName(worldName);
        var bytes = tex.EncodeToPNG();
        UnityEngine.Object.Destroy(tex);

        _ = File.WriteAllBytesAsync(thumbPath, bytes);
    }

    private static WorldData GetSaveDataByPath(string path)
    {
        if (!File.Exists(path)) {
            Debug.Log($"[{nameof(WorldSaveSystem)}] Save file not found in " + path);
            return null;
        }

        var json = File.ReadAllText(path);
        var worldData = WorldDataMigrator.GetWorldData(json);

        return worldData;
    }

    private static string GetSavesFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, "Worlds");
    }

    private static string GetSaveFolderPathByName(string worldName)
    {
        if (string.IsNullOrEmpty(worldName))
            return GetSavesFolderPath();

        var path = Path.Combine(Application.persistentDataPath, worldName);

        return path;
    }

    private static string GetSaveFilePathByName(string worldName)
    {
        return Path.Combine(GetSaveFolderPathByName(worldName), worldName + saveFileExtension);
    }

    private static string GetSaveThumbPathByName(string worldName)
    {
        if (string.IsNullOrEmpty(worldName))
            return Path.Combine(GetSavesFolderPath(), ".png");

        var path = Path.Combine(GetSaveFolderPathByName(worldName), worldName + ".png");
        if (path == null) {
            Debug.LogError($"SaveThumbPath is not valid by name {worldName}!");
        }

        return path;
    }
}