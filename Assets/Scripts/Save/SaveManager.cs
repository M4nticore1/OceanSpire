using UnityEngine.SceneManagement;

public class SaveManager
{
    public static SaveManager Instance { get; private set; }

    public SaveData[] allSaveData { get; private set; }
    public SaveData saveData { get; private set; }
    public string saveWorldName { get; private set; }

    public SaveManager()
    {
        if (Instance != null) return;

        Instance = this;
    }

    public void Initialize()
    {
        EventBus.Instance.onCreateWorldButtonClicked += CreateWorld;
        EventBus.Instance.onLoadWorldButtonClicked += LoadWorld;
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
