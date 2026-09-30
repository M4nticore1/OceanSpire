using System.Collections.Generic;
using UnityEngine;

public class WorldSavesHandler : MonoBehaviour
{
    public static WorldSavesHandler Instance { get; private set; }

    public List<WorldData> AllSavesData { get; private set; } = new();
    public WorldData CurrentWorldData { get; private set; } = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            //Debug.Log($"[{nameof(WorldSavesHandler)}] Another instance detected. Destroying duplicate on scene: {gameObject.scene.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UpdateSavesData();
    }

    private void OnEnable()
    {
        WorldSaveSystem.OnWorldSaveCreated += HandleWorldDataAdded;
        WorldSaveSystem.OnWorldSaveDeleted += HandleWorldDataDeleted;
    }

    private void OnDisable()
    {
        WorldSaveSystem.OnWorldSaveCreated -= HandleWorldDataAdded;
        WorldSaveSystem.OnWorldSaveDeleted -= HandleWorldDataDeleted;
    }

    // World
    public void SetWorldData(WorldData data)
    {
        CurrentWorldData = data;
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