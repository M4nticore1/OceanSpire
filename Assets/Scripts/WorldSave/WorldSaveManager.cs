using UnityEngine;

public class WorldSaveManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private ElevatorCabinsManager elevatorCabinsManager;
    [SerializeField] private DockPointsManager dockPointsManager;
    [SerializeField] private BoatsManager boatsManager;
    [SerializeField] private CreaturesManager creaturesManager;
    [SerializeField] private DriftingLootManager driftingLootManager;
    [SerializeField] private CityStorage cityStorgae;
    [SerializeField] private DailyTasksManager dailyTasksManager;
    [SerializeField] private DailyRewardManager dailyRewardManager;
    [SerializeField] private RaidManager raidManager;
    [SerializeField] private WanderersManager wanderersManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private WindManager windManager;

    public void SaveWorld()
    {
        var worldData = WorldData.Create(
            WorldSaveHandler.Instance,
            playerController,
            buildingsManager, elevatorCabinsManager,
            dockPointsManager,
            boatsManager,
            creaturesManager,
            driftingLootManager,
            cityStorgae.Inventory,
            dailyTasksManager,
            dailyRewardManager,
            raidManager,
            wanderersManager,
            tutorialManager,
            windManager);

        WorldSaveSystem.SaveWorld(worldData);
        WorldSaveHandler.Instance.SetWorldData(worldData);
    }
}