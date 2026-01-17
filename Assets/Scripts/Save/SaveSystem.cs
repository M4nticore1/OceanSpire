using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    public static SaveData saveData = null;
    private static string worldSavesPath = Application.persistentDataPath + "/OceanSpire.sav";

    public static void SaveData(PlayerController playerController, GameManager gameManager)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        if (File.Exists(worldSavesPath))
            stream = new FileStream(worldSavesPath, FileMode.Create);
        else
            stream = new FileStream(worldSavesPath, FileMode.CreateNew);

        SaveData playerData = new SaveData(playerController);

        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static SaveData LoadData()
    {
        if (File.Exists(worldSavesPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(worldSavesPath, FileMode.Open);

            if (stream.Length > 0)
            {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();

                Debug.Log(worldSavesPath);

                saveData = data;
                return data;
            }
            else
            {
                Debug.Log("Stream is empty");
                return null;
            }
        }
        else
        {
            Debug.Log("Save file not found in " + worldSavesPath);
            return null;
        }
    }

    public static void DeleteSave(int saveIndex)
    {
        if (File.Exists(worldSavesPath))
        {
            File.Delete(worldSavesPath);
            saveData = null;
            Debug.Log("Сохранение удалено!");
        }
        else
        {
            Debug.Log("Файл сохранения не найден.");
        }
    }
}
