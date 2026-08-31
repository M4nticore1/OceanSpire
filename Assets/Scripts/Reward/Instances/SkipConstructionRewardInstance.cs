using System.Collections.Generic;
using UnityEngine;

public class SkipConstructionRewardInstance : RewardInstance
{
    private ConstructionComponent constructionComponent;

    public SkipConstructionRewardInstance(AdRewardDefinition definition, ConstructionComponent constructionComponent) : base(definition, 0)
    {
        this.constructionComponent = constructionComponent;
    }

    protected override void HandleRewardRecieved()
    {
        base.HandleRewardRecieved();

        constructionComponent.FinishConstruction();
    }
}
