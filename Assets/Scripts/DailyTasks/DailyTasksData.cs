using UnityEngine;

public class DailyTasksData
{
    public DailyTaskInstanceData[] Tasks { get; private set; }
    public long NextRestTime { get; private set; }
    public bool IsAdUpdateUsed { get; private set; }

    public static DailyTasksData Create(DailyTasksManager manager)
    {
        return new DailyTasksData()
        {
            Tasks = DailyTasksSaveSystem.SaveTasks(manager),
            NextRestTime = manager.NextRestTime,
            IsAdUpdateUsed = manager.IsAdUpdateUsed,
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