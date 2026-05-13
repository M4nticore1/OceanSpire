using UnityEngine;

public class DailyTasksLoader : Loader
{
    [SerializeField] private DailyTasksManager dailyTasksManager;

    protected override void Load(WorldData worldData)
    {
        if (worldData != null && worldData.DailyTasks != null) {
            LoadTasks(worldData.DailyTasks);
        }
        else {
            InitTasks();
        }
    }

    private void LoadTasks(DailyTasksData dailyTasksData)
    {
        dailyTasksManager.Init(dailyTasksData);
    }

    private void InitTasks()
    {
        DailyTasksData dailyTasksData = new DailyTasksData()
        {
            Tasks = dailyTasksManager.GetRandomTasksData(),
            NextResetTime = dailyTasksManager.CalculateNextResetTime(),
            AdUpdateUsed = false
        };

        dailyTasksManager.Init(dailyTasksData);
    }
}