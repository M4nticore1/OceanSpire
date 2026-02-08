using UnityEngine.SceneManagement;

public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance => _instance ??= new SaveManager();

    public SaveData[] allSaveData { get; private set; }
    public SaveData saveData { get; private set; }
    public string saveWorldName { get; private set; }

    private SaveManager() { }

    public void Initialize()
    {
        EventBus.onCreateWorldButtonClicked += CreateWorld;
        EventBus.onLoadWorldButtonClicked += LoadWorld;
        FindSavesData();
    }

    public void FindSavesData()
    {
        allSaveData = SaveSystem.GetAllSaveData();
    }

    public void SetSaveData(SaveData data)
    {
        saveData = data;
        SetSaveWorldName(saveData.cityData.cityName);
    }

    public void SetSaveWorldName(string name)
    {
        saveWorldName = name;
    }

    public void CreateWorld(string worldName)
    {
        SetSaveWorldName(worldName);
        SceneManager.LoadScene(1);
    }

    public void LoadWorld(SaveData data)
    {
        SetSaveData(data);
        SceneManager.LoadScene(1);
    }
}
