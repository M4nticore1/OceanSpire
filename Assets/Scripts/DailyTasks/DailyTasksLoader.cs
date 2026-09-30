using UnityEngine;

public class DailyTasksLoader : WorldLoader
{
    [SerializeField] private DailyTasksManager dailyTasksManager;

    protected override void Load(WorldData worldData)
    {
        var data = worldData != null ? worldData.DailyTasks : null;

        if (data != null) {
            dailyTasksManager.Init(data);
        }
        else {
            dailyTasksManager.Init();
        }
    }
}