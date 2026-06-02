using UnityEngine;

public class AutoSave : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private ElevatorCabinsManager elevatorCabinsManager;
    [SerializeField] private DockPointsManager dockPointsManager;
    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private CityStorage cityStorgae;
    [SerializeField] private DailyTasksManager dailyTasksManager;
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private RaidManager raidManager;
    [SerializeField] private WanderersManager wanderersManager;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Auto Save")]
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

        WorldData worldData = WorldData.Create(WorldSaveManager.Instance,
            buildingsManager, elevatorCabinsManager,
            dockPointsManager,
            boatsManager,
            creaturesManager,
            cityStorgae.Inventory,
            dailyTasksManager,
            dailyRewardManager,
            raidManager,
            wanderersManager,
            tutorialManager);

        WorldSaveSystem.SaveWorld(worldData);
        WorldSaveManager.Instance.SetWorldData(worldData);

        crrentSaveDataTime = 0f;
    }

    private void TickSaveScreeshot()
    {
        crrentSaveThumbTime += Time.deltaTime;
        if (crrentSaveThumbTime < autoSaveThumbFrequency) return;

        WorldSaveSystem.SaveWorldThumb(WorldSaveManager.Instance.SaveWorldName);
        crrentSaveThumbTime = 0f;
    }
}