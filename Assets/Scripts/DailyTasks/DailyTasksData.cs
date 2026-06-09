using UnityEngine;

public class DailyTasksData
{
    public DailyTaskInstanceData[] Tasks = null;
    public long NextResetTime = 0;
    public bool AdUpdateUsed = false;
    public bool TasksViewed = false;

    public static DailyTasksData Create(DailyTasksManager manager)
    {
        return new DailyTasksData()
        {
            Tasks = DailyTasksSaveSystem.SaveTasks(manager),
            NextResetTime = manager.NextRestTime,
            AdUpdateUsed = manager.IsAdUpdateUsed,
            TasksViewed = manager.IsDailyTasksViewed,
        };
    }
}

public static class DailyTasksSaveSystem
{
    public static DailyTaskInstanceData[] SaveTasks(DailyTasksManager manager)
    {
        DailyTaskInstanceData[] tasks = new DailyTaskInstanceData[manager.CurrentTasks.Count];

        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = DailyTaskInstanceData.Create(manager.CurrentTasks[i]);
        }

        return tasks;
    }
}