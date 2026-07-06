using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DailyTaskInstanceData
{
    public int Id = 0;
    public int Progress = 0;
    public bool Completed = false;

    public static DailyTaskInstanceData Create(DailyTaskInstance task)
    {
        return new DailyTaskInstanceData()
        {
            Id = task.Id,
            Progress = task.Progress,
            Completed = task.IsCompleted
        };
    }

    public static List<DailyTaskInstanceData> Create(DailyTaskInstance[] tasks)
    {
        var tasksData = new List< DailyTaskInstanceData>();

        foreach (var task in tasks) {
            var taskData = Create(task);
            tasksData.Add(taskData);
        }

        return tasksData;
    }
}