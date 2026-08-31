using UnityEngine;

public class ReviveAdRewardInstance : RewardInstance
{
    private Human citizen;

    public ReviveAdRewardInstance(ReviveAdRewardDefinition definition, Citizen citizen) : base(definition, 0)
    {
        this.citizen = citizen;
    }

    protected override void HandleRewardRecieved()
    {
        base.HandleRewardRecieved();

        if (!citizen) {
            Debug.LogError("citizen is not valid to revive");
            return;
        }

        citizen.ReviveComponent.Revive();
        citizen.SelectComponent.Select();
        ReviveManager.Instance.RemoveReviveCount();
    }

    public void SetHuman(Citizen citizen)
    {
        this.citizen = citizen;
    }
}