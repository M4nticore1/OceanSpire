using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class DailyTasksData
{
    public DailyTaskInstanceData[] Tasks = new DailyTaskInstanceData[0];
    public long NextResetTime = 0;
    public bool AdUpdateUsed = false;
    public bool TasksViewed = false;

    public static DailyTasksData Create(DailyTasksManager manager)
    {
        return new DailyTasksData()
        {
            Tasks = DailyTaskInstanceData.Create(manager.CurrentTasks.ToArray()).ToArray(),
            NextResetTime = manager.NextResetTime,
            AdUpdateUsed = manager.IsAdUpdateUsed,
            TasksViewed = manager.IsDailyTasksViewed,
        };
    }
}