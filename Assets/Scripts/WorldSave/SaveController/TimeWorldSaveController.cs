using UnityEngine;

public class TimeWorldSaveController : WorldSaveController
{
    [SerializeField] private float autoSaveDataFrequency = 5f;
    [SerializeField] private float autoSaveThumbFrequency = 60f;

    private float crrentSaveDataTime = 0f;
    private float crrentSaveThumbTime = 0f;

    private void Start()
    {
        crrentSaveThumbTime = autoSaveThumbFrequency - autoSaveDataFrequency;
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

        SaveWorld();

        crrentSaveDataTime = 0f;
    }

    private void TickSaveScreeshot()
    {
        crrentSaveThumbTime += Time.deltaTime;
        if (crrentSaveThumbTime < autoSaveThumbFrequency) return;

        WorldSaveSystem.SaveWorldThumb(WorldSaveHandler.Instance.SaveWorldName);
        crrentSaveThumbTime = 0f;
    }
}