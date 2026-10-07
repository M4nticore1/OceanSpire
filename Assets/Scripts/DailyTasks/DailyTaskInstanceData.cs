using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DailyTaskInstanceData
{
    public int Id = 0;
    public int RewardId = 0;
    public int Progress = 0;
    public bool Completed = false;

    public static DailyTaskInstanceData Create(DailyTaskInstance task)
    {
        if (task == null) {
            Debug.LogError("Task is not valid!");
            return null;
        }

        return new DailyTaskInstanceData()
        {
            Id = task.Id,
            RewardId = task.RewardId,
            Progress = task.TaskProgress,
            Completed = task.IsCompleted
        };
    }

    public static List<DailyTaskInstanceData> Create(DailyTaskInstance[] tasks)
    {
        var tasksData = new List<DailyTaskInstanceData>();

        if (tasks == null) {
            Debug.LogError("Tasks is not valid!");
            return tasksData;
        }

        foreach (var task in tasks) {
            if (task == null) continue;

            var taskData = Create(task);
            tasksData.Add(taskData);
        }

        return tasksData;
    }
}