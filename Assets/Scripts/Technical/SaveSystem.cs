using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    public static SaveData saveData = null;
    private static string path = Application.persistentDataPath + "/OceanSpire.sav";

    public static void SaveData(PlayerController playerController, CityManager cityManger)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        if (File.Exists(path))
            stream = new FileStream(path, FileMode.Create);
        else
            stream = new FileStream(path, FileMode.CreateNew);

        SaveData playerData = new SaveData(playerController, cityManger);

        formatter.Serialize(stream, playerData);
        stream.Close();
    }

    public static SaveData LoadData()
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            if (stream.Length > 0)
            {
                SaveData data = formatter.Deserialize(stream) as SaveData;
                stream.Close();

                Debug.Log(path);

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
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }

    public static void DeleteSave(int saveIndex)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            saveData = null;
            Debug.Log("Сохранение удалено!");
        }
        else
        {
            Debug.Log("Файл сохранения не найден.");
        }
    }
}
