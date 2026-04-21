using UnityEngine;

public class TimeDailyTaskCondition : DailyTaskCondition
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

        InvokeProgressChanged(1);
        currentTime = 0f;
    }
}