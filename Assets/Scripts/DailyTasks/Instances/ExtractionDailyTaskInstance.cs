using System;
using UnityEngine;

[Serializable]
public class ExtractionDailyTaskInstance : DailyTaskInstance
{
    private ItemDefinition conditionItemDefinition;

    public ExtractionDailyTaskInstance(ExtractionDailyTaskDefinition definition, ItemInstance reward, int progress, bool completed)
    {
        if (definition == null) {
            Debug.LogError("ExtractionDefinition is not valid!");
            return;
        }
        if (reward == null) {
            Debug.LogError("Reward is not valid!");
        }

        conditionItemDefinition = definition.GetRandomConditionItemDefinition();
        Definition = definition;
        Reward = reward;
        ConditionAmount = definition.GetConditionByGameStage(this);
        TaskProgress = progress;
        IsCompleted = completed;
    }

    public override LocalizationItem GetConditionName()
    {
        if (conditionItemDefinition == null) return null;

        return conditionItemDefinition.NameLocalizationItem;
    }

    public override Sprite GetConditionIcon()
    {
        if (conditionItemDefinition == null) return null;

        return conditionItemDefinition.ItemIcon;
    }
}