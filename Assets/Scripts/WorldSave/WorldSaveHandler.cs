using System.Collections.Generic;
using UnityEngine;

public class WorldSaveHandler : MonoBehaviour
{
    public static WorldSaveHandler Instance { get; private set; }

    public List<WorldData> AllSavesData { get; private set; }
    public WorldData CurrentWorldData { get; private set; }
    public string SaveWorldName { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null) {
            Destroy(gameObject);
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
        AllSavesData = WorldSaveSystem.GetAllSaveData();
    }

    private void HandleWorldDataAdded(WorldData worldData)
    {
        if (worldData == null) return;
        if (AllSavesData.Contains(worldData)) return;

        AllSavesData.Add(worldData);
    }

    private void HandleWorldDataDeleted(WorldData worldData)
    {
        if (worldData == null) return;

        AllSavesData.Remove(worldData);
    }
}