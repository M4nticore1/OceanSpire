using UnityEngine;

public class AcceptResidentDailyTaskController : DailyTaskController
{
    private WandererAdmissionManager admissionManager => WandererAdmissionManager.Instance;

    protected override bool Subscribe()
    {
        if (admissionManager == null) return false;

        admissionManager.OnWandererAccepted += HandleWandererAccepted;

        return true;
    }

    protected override bool Unsubscribe()
    {
        if (admissionManager == null) return false;

        admissionManager.OnWandererAccepted -= HandleWandererAccepted;

        return true;
    }

    private void HandleWandererAccepted(Citizen citizen)
    {
        if (citizen == null) return;

        AddTaskProgress(1);
    }
}