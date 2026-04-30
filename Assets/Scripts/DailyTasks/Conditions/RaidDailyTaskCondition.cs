using UnityEngine;

public class RaidDailyTaskCondition : DailyTaskCondition
{
    protected override bool Subscribe()
    {
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.onRaidEnded += OnRaidEnded;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.onRaidEnded -= OnRaidEnded;

        return true;
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        if (!result.isRepeled) return;

        InvokeProgressChanged(1);
    }
}