using UnityEngine;

public class SaveManager
{
    private static SaveManager instance;
    public static SaveManager Instance => instance ??= new SaveManager();

    public SaveData[] allSaveData { get; private set; }
    public SaveData saveData { get; private set; }
    public string saveWorldName { get; private set; }

    public void Initialize()
    {
        Debug.Log("Initialize");
        FindSavesData();
    }

    private void FindSavesData()
    {
        allSaveData = SaveSystem.GetAllSaveData();
    }

    public void SetSaveData(SaveData data)
    {
        saveData = data;
        SetSaveWorldName(saveData.worldName);
    }

    public void SetSaveWorldName(string name)
    {
        saveWorldName = name;
    }
}
