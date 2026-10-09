using System;
using UnityEngine;

[Serializable]
public class SimpleDailyTaskInstance : DailyTaskInstance
{
    private SimpleDailyTaskDefinition SimpleDailyTaskDefinition => Definition as SimpleDailyTaskDefinition;

    public SimpleDailyTaskInstance(DailyTaskDefinition definition, ItemInstance reward, int progress, bool completed)
    {
        if (definition == null) {
            Debug.LogError("Definition is not valid!");
        }
        if (reward == null) {
            Debug.LogError("Reward is not valid!");
        }

        Definition = definition;
        Reward = reward;
        ConditionAmount = definition.GetConditionAmount(reward?.Definition);
        TaskProgress = progress;
    }

    public override Sprite GetConditionIcon()
    {
        if (SimpleDailyTaskDefinition == null) return null;

        return SimpleDailyTaskDefinition.ConditionImage;
    }
}