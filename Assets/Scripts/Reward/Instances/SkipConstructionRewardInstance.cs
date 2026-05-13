using System.Collections.Generic;
using UnityEngine;

public class SkipConstructionRewardInstance : RewardInstance
{
    private ConstructionComponent constructionComponent;

    public SkipConstructionRewardInstance(ConstructionComponent constructionComponent)
    {
        this.constructionComponent = constructionComponent;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return new();
    }

    protected override void OnRewardRecieved()
    {
        base.OnRewardRecieved();

        constructionComponent.FinishConstruction();
    }
}
