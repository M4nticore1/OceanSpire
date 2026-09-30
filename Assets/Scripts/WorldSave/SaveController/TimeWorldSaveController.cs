using UnityEngine;

public class TimeWorldSaveController : WorldSaveController
{
    [Header("Params")]
    [SerializeField] private float autoSaveDataFrequency = 5f;
    [SerializeField] private float autoSaveThumbFrequency = 60f;

    [Header("Check")]
    [SerializeField] private float crrentSaveDataTime = 0f;
    [SerializeField] private float crrentSaveThumbTime = 0f;

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
        if (crrentSaveThumbTime < autoSaveThumbFrequency)
            return;

        crrentSaveThumbTime = 0f;

        var currentData = WorldSavesHandler.Instance.CurrentWorldData;
        if (currentData == null)
            return;

        WorldSaveSystem.SaveWorldThumb(currentData.WorldName);
    }
}