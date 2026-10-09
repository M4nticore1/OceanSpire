using UnityEngine;

public class RaidDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.OnRaidEnded += OnRaidEnded;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.OnRaidEnded -= OnRaidEnded;

        return true;
    }

    private void OnRaidEnded(RaidEndedResult result)
    {
        //if (!result.IsRepeled) return;

        AddTaskProgress(1);
    }
}