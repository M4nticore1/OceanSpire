using UnityEngine;

public class ReviveCitizenAdReward : AdRewardInstance
{
    private Human human;

    public ReviveCitizenAdReward(Human human)
    {
        this.human = human;
    }

    protected override void OnRewardRecieved()
    {
        human.Health.Revive();
    }
}