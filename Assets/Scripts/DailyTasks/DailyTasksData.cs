using UnityEngine;

public class DailyTasksDataV1
{
    public int[] taskIds { get; private set; }
    public int[] taskProgresses { get; private set; }
    public int nextUpdateTime { get; private set; }
    public bool adUpdateUsed { get; private set; }

    public DailyTasksDataV1(int[] taskIds, int[] taskProgresses, int nextUpdateTime, bool usedAdUpdate)
    {
        this.taskIds = taskIds;
        this.taskProgresses = taskProgresses;
        this.nextUpdateTime = nextUpdateTime;
        this.adUpdateUsed = usedAdUpdate;
    }
}