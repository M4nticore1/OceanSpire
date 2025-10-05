using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem
{
    private static string path = Application.persistentDataPath + "/OceanSpire.sav";

    public static void SaveData(PlayerController playerController, CityManager cityManger)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

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
}
