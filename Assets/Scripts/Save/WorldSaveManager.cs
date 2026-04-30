using UnityEngine.SceneManagement;

public class WorldSaveManager
{
    private static WorldSaveManager instance;
    public static WorldSaveManager Instance
    {
        get
        {
            if (instance == null) {
                instance = new WorldSaveManager();
                instance.Init();
            }

            return instance;
        }
    }

    public WorldData[] allSaveData { get; private set; }
    public WorldData currentSaveWorldData { get; private set; }
    public string saveWorldName { get; private set; }

    private WorldSaveManager() { }

    private void Init()
    {
        EventBus.onCreateWorldButtonClicked += CreateWorld;
        EventBus.onLoadWorldButtonClicked += LoadWorld;
        FindSavesData();
    }

    // World
    public void FindSavesData()
    {
        allSaveData = WorldSaveSystem.GetAllSaveData();
    }

    public void SetWorldData(WorldData data)
    {
        currentSaveWorldData = data;
        SetSaveWorldName(currentSaveWorldData.cityData.cityName);
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

    public void LoadWorld(WorldData data)
    {
        SetWorldData(data);
        SceneManager.LoadScene(1);
    }
}
