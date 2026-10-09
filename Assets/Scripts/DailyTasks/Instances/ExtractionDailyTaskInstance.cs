using System;
using System.Collections.Generic;
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

        Definition = definition;
        Reward = reward;
        conditionItemDefinition = definition.GetRandomConditionItemDefinition(reward?.Definition);
        ConditionAmount = definition.GetConditionAmountByConditionItem(conditionItemDefinition);
        TaskProgress = progress;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        var dict = base.GetLocalization();

        dict.Add("conditionName", LocalizationManager.Instance.GetLocalizedText(conditionItemDefinition.NameLocalizationItem));

        return dict;
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