using System.Collections.Generic;

public class ReviveAdRewardInstance : AdRewardInstance
{
    private Human human;

    public ReviveAdRewardInstance(ReviveAdRewardDefinition definition, float limitTime, Human human) : base(definition, limitTime)
    {
        this.human = human;
    }

    public override Dictionary<string, string> GetLocalizations()
    {
        return new Dictionary<string, string>()
        {

        };
    }

    protected override void OnRewardRecieved()
    {
        human.ReviveHandler.Revive();
    }

    public void SetHuman(Human human)
    {
        this.human = human;
    }
}