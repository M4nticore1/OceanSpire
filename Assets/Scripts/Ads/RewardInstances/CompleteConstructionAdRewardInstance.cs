using System.Collections.Generic;
using UnityEngine;

public class CompleteConstructionAdRewardInstance : AdRewardInstance
{
    private ConstructionComponent constructionComponent;

    public CompleteConstructionAdRewardInstance(ConstructionComponent constructionComponent)
    {
        this.constructionComponent = constructionComponent;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return null;
    }

    protected override void OnRewardRecieved()
    {
        constructionComponent.FinishConstruction();
    }
}
