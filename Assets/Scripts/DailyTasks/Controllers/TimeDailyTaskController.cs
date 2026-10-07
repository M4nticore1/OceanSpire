using UnityEngine;

public class TimeDailyTaskController : DailyTaskController
{
    private float currentTime = 0f;

    protected override bool Subscribe()
    {
        return true;
    }

    protected override bool Unsubscribe()
    {
        return true;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime < 60f) return;

        AddTaskProgress(1);
        currentTime = 0f;
    }
}