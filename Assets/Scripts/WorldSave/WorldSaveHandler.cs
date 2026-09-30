using UnityEngine;

public class WorldSaveHandler : MonoBehaviour
{
    public static WorldSaveHandler Instance { get; private set; }

    public WorldData[] AllSaveData { get; private set; }
    public WorldData CurrentWorldData { get; private set; }
    public string SaveWorldName { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError($"[{nameof(WorldSaveHandler)}] There's another WorldSaveHandler on scene!");
        }
        else {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        WorldSaveSystem.OnWorldDataAdded += HandleWorldDataAdded;
        WorldSaveSystem.OnWorldDataDeleted += HandleWorldDataDeleted;
    }

    private void OnDisable()
    {
        WorldSaveSystem.OnWorldDataAdded -= HandleWorldDataAdded;
        WorldSaveSystem.OnWorldDataDeleted -= HandleWorldDataDeleted;
    }

    private void Start()
    {
        UpdateSavesData();
    }

    // World
    public void SetWorldData(WorldData data)
    {
        CurrentWorldData = data;
        SetSaveWorldName(CurrentWorldData.WorldName);
    }

    public void SetSaveWorldName(string name)
    {
        SaveWorldName = name;
    }

    private void UpdateSavesData()
    {
        AllSaveData = WorldSaveSystem.GetAllSaveData();
    }

    private void HandleWorldDataAdded(WorldData worldData)
    {
        UpdateSavesData();
    }

    private void HandleWorldDataDeleted(WorldData worldData)
    {
        UpdateSavesData();
    }
}
