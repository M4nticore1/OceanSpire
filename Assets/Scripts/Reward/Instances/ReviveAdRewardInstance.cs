using System.Collections.Generic;

public class ReviveAdRewardInstance : RewardInstance
{
    private Human human;

    public ReviveAdRewardInstance(ReviveAdRewardDefinition definition, Human human) : base(definition, 0)
    {
        this.human = human;
    }

    protected override void OnRewardRecieved()
    {
        base.OnRewardRecieved();

        human.ReviveComponent.Revive();
        human.SelectComponent.Select();
        ReviveManager.Instance.RemoveReviveCount();
    }

    public void SetHuman(Human human)
    {
        this.human = human;
    }
}