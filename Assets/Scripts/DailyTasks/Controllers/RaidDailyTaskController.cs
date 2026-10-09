using UnityEngine;

public class RaidDailyTaskController : DailyTaskController
{
    protected override bool Subscribe()
    {
        if (!base.Subscribe()) return false;
        if (!RaidManager.Instance) return false;

        RaidManager.Instance.OnRaidEnded += OnRaidEnded;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (!base.Unsubscribe()) return false;
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