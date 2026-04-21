using UnityEngine;

public class RaidDailyTaskCondition : DailyTaskCondition
{
    protected override bool Subscribe()
    {
        RaidManager.onRaidEnded += OnRaidEnded;

        return true;
    }

    protected override bool Unsubscribe()
    {
        RaidManager.onRaidEnded -= OnRaidEnded;

        return true;
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        if (!result.isRepeled) return;

        InvokeProgressChanged(1);
    }
}