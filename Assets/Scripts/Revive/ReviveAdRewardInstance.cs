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

        if (citizen == null) {
            Debug.LogError($"[{nameof(ReviveAdRewardInstance)}] Citizen is not valid!");
            return;
        }

        citizen?.ReviveComponent.Revive();
        citizen?.SelectComponent.Select();
    }

    public void SetHuman(Citizen citizen)
    {
        this.citizen = citizen;
    }
}