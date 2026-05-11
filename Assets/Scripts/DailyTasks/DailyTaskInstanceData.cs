using UnityEngine;

public class DailyTaskInstanceData
{
    public int Id = 0;
    public int Progress = 0;
    public bool IsCompleted = false;

    public static DailyTaskInstanceData Create(DailyTaskInstance task)
    {
        return new DailyTaskInstanceData()
        {
            Id = (int)task.Definition.TaskId,
            Progress = task.Progress,
            IsCompleted = task.IsCompleted,
        };
    }
}
