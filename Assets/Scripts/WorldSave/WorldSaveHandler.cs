using UnityEngine;

public class WorldSaveHandler
{
    private static WorldSaveHandler instance;
    public static WorldSaveHandler Instance
    {
        get
        {
            if (instance == null) {
                instance = new WorldSaveHandler();
                instance.Init();
            }

            return instance;
        }
    }

    public WorldData[] AllSaveData { get; private set; }
    public WorldData CurrentWorldData { get; private set; }
    public string SaveWorldName { get; private set; }

    private WorldSaveHandler() { }

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
