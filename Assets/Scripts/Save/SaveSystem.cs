using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    private static string saveFileExtension = ".sav";
    private static string worldSavesPath = Path.Combine(Application.persistentDataPath, "Saves");
    //private static string worldSavePath = Path.Combine(Application.persistentDataPath, "OceanSpire.sav");

    public static void CreateSave(string name)
    {

    }

    public static void SaveData(PlayerController playerController)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        string worldName = SaveManager.Instance.saveWorldName;
        string path = Path.Combine(worldSavesPath, worldName + saveFileExtension);

        if (!Directory.Exists(worldSavesPath)) {
            Directory.CreateDirectory(worldSavesPath);
        }

        if (File.Exists(path))
            stream = new FileStream(path, FileMode.Create);
        else
            stream = new FileStream(path, FileMode.CreateNew);

        SaveData playerData = new SaveData(playerController);
        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static SaveData GetSaveDataByWorldName(string worldName)
    {
        string path = Path.Combine(worldSavesPath, worldName);
        return GetSaveDataByPath(path);
    }

    private static SaveData GetSaveDataByPath(string path)
    {
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
        if (Directory.Exists(worldSavesPath)) {
            string[] filePaths = Directory.GetFiles(worldSavesPath, $"*{saveFileExtension}");
            List<SaveData> datas = new List<SaveData>();
            for (int i = 0; i < filePaths.Length; i++) {
                string filePath = filePaths[i];
                SaveData data = GetSaveDataByPath(filePath);
                if (data != null)
                    datas.Add(data);
            }
            Debug.Log(datas.Count);
            return datas.ToArray();
        }
        else {
            Debug.Log("Save file not found in " + worldSavesPath);
            return new SaveData[0];
        }
    }

    public static void RemoveSave(string worldName)
    {
        string path = Path.Combine(worldSavesPath, worldName + saveFileExtension);
        if (File.Exists(path)) {
            File.Delete(path);
            Debug.Log("Сохранение удалено!");
        }
        else {
            Debug.Log("Файл сохранения не найден.");
        }
    }
}
