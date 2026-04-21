using UnityEngine;

public class DailyTasksDataV1
{
    public int[] taskIds { get; private set; }
    public int[] taskProgresses { get; private set; }

    public DailyTasksDataV1(int[] taskIds, int[] taskProgresses)
    {
        this.taskIds = taskIds;
        this.taskProgresses = taskProgresses;
    }
}