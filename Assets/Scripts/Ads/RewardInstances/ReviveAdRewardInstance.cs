using System.Collections.Generic;

public class ReviveAdRewardInstance : AdRewardInstance
{
    private Human human;

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
        human.ReviveComponent.Revive();
        ReviveManager.Instance.RemoveReviveCount();
    }

    public void SetHuman(Human human)
    {
        this.human = human;
    }
}