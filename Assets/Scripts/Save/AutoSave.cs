using UnityEngine;

public class AutoSave : MonoBehaviour
{
    [SerializeField] private float autoSaveDataFrequency = 5f;
    [SerializeField] private float autoSaveThumbFrequency = 60f;

    private float crrentSaveDataTime = 0f;
    private float crrentSaveThumbTime = 0f;

    private void Start()
    {
        crrentSaveDataTime = autoSaveDataFrequency;
        crrentSaveThumbTime = autoSaveThumbFrequency;
    }

    private void Update()
    {
        TickSaveData();
        TickSaveScreeshot();
    }

    private void TickSaveData()
    {
        crrentSaveDataTime += Time.deltaTime;
        if (crrentSaveDataTime < autoSaveDataFrequency) return;

        WorldData worldData = WorldData.Create(WorldSaveManager.Instance, BuildingsManager.Instance, DockPointsManager.Instance, BoatsManager.Instance, CreaturesManager.Instance, CityStorage.Instance.Inventory, DailyTasksManager.Instance, DailyRewardManager.Instance, RaidManager.Instance);
        WorldSaveSystem.SaveWorld(worldData);
        WorldSaveManager.Instance.SetWorldData(worldData);

        crrentSaveDataTime = 0f;
    }

    private void TickSaveScreeshot()
    {
        crrentSaveThumbTime += Time.deltaTime;
        if (crrentSaveThumbTime < autoSaveThumbFrequency) return;

        WorldSaveSystem.SaveWorldThumb(WorldSaveManager.Instance.CurrentWorldData);
        crrentSaveThumbTime = 0f;
    }
}