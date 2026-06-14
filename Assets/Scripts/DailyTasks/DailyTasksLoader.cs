using UnityEngine;

public class DailyTasksLoader : WorldLoader
{
    [SerializeField] private DailyTasksManager dailyTasksManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.DailyTasks != null) {
            dailyTasksManager.Init(worldData.DailyTasks);
        }
        else {
            dailyTasksManager.Init();
        }
    }
}