using UnityEngine;

public class DailyTasksDataV1
{
    public int[] TaskIds { get; private set; }
    public int[] TaskProgresses { get; private set; }
    public long NextUpdateTime { get; private set; }
    public bool AdUpdateUsed { get; private set; }

    public DailyTasksDataV1(int[] taskIds, int[] taskProgresses, long nextUpdateTime, bool adUpdateUsed)
    {
        TaskIds = taskIds;
        TaskProgresses = taskProgresses;
        NextUpdateTime = nextUpdateTime;
        AdUpdateUsed = adUpdateUsed;
    }
}