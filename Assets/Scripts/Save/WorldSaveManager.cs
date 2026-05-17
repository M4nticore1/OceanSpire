using UnityEngine;

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

    public WorldData[] AllSaveData { get; private set; }
    public WorldData CurrentWorldData { get; private set; }
    public string SaveWorldName { get; private set; }

    private WorldSaveManager() { }

    private void Init()
    {
        FindSavesData();
    }

    // World
    public void FindSavesData()
    {
        AllSaveData = WorldSaveSystem.GetAllSaveData();
    }

    public void SetWorldData(WorldData data)
    {
        CurrentWorldData = data;
        SetSaveWorldName(CurrentWorldData.WorldName);
    }

    public void SetSaveWorldName(string name)
    {
        SaveWorldName = name;
    }
}
