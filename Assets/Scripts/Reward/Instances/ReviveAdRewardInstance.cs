using System.Collections.Generic;

public class ReviveAdRewardInstance : RewardInstance
{
    private Human human;

    public ReviveAdRewardInstance(Human human) : base()
    {
        this.human = human;
    }

    public ReviveAdRewardInstance(ReviveAdRewardDefinition definition, Human human) : base(definition)
    {
        this.human = human;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {

        };
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