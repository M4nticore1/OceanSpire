using UnityEngine;

public class AcceptResidentDailyTaskController : DailyTaskController
{
    private WandererAdmissionManager admissionManager => WandererAdmissionManager.Instance;

    protected override bool Subscribe()
    {
        admissionManager.OnWandererAccepted += HandleWandererAccepted;

        return true;
    }

    protected override bool Unsubscribe()
    {
        admissionManager.OnWandererAccepted -= HandleWandererAccepted;

        return true;
    }

    private void HandleWandererAccepted(Wanderer wanderer, Citizen citizen)
    {

    }
}