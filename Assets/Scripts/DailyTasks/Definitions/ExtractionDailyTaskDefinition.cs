using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ExtractionTaskEntry
{
    [SerializeField] private ItemDefinition conditionItemDefinition;
    public ItemDefinition ConditionItemDefinition => conditionItemDefinition;

    [SerializeField] private int minConditionAmount = 0;
    public int MinConditionAmount => minConditionAmount;

    [SerializeField] private int maxConditionAmount = 0;
    public int MaxConditionAmount => maxConditionAmount;
}

[CreateAssetMenu(fileName = "ExtractionDailyTaskDefinition", menuName = "Daily Tasks/ExtractionDailyTaskDefinition")]
public class ExtractionDailyTaskDefinition : DailyTaskDefinition
{
    [Header("Extraction")]
    [SerializeField] private List<ExtractionTaskEntry> taskConditions = new();
    public IReadOnlyList<ExtractionTaskEntry> TaskConditions => taskConditions;

    public override ItemInstance GetRandomReward()
    {
        var index = UnityEngine.Random.Range(0, RandomRewards.Length);
        var randomReward = RandomRewards[index];

        if (randomReward == null) {
            Debug.LogError($"RandomReward is not valid by index {index} at {this}!");
            return null;
        }

        var reward = new ItemInstance(randomReward.Definition);
        var gameStage = GameStageSystem.CalculateGameStagePercent();

        var minAmount = randomReward.MinAmount;
        var maxAmount = randomReward.MaxAmount;
        var amount = (int)Mathf.Lerp(minAmount, maxAmount, gameStage);

        reward.SetAmount(amount);

        return reward;
    }

    public override int GetConditionAmount(ItemDefinition currentRewardItemDefinition)
    {
        if (currentRewardItemDefinition == null) {
            Debug.LogError("ItemDefinition is not valid!");
            return 0;
        }

        var conditions = GetExcludedTasksList(currentRewardItemDefinition);

        if (conditions.Count == 0) {
            Debug.LogError("Conditions list is empty after filtering!");
            return 0;
        }

        var index = UnityEngine.Random.Range(0, conditions.Count);
        var finalCondition = conditions[index];

        if (finalCondition == null) {
            Debug.LogError($"Condition is not valid by index {index}!");
            return 0;
        }

        var gameStage = GameStageSystem.CalculateGameStagePercent();
        var amount = (int)Mathf.Lerp(finalCondition.MinConditionAmount, finalCondition.MaxConditionAmount, gameStage);

        return amount;
    }

    public override DailyTaskInstance CreateInstance(ItemInstance reward, int progress = 0, bool completed = false)
    {
        return new ExtractionDailyTaskInstance(this, reward, progress, completed);
    }

    public int GetConditionAmountByConditionItem(ItemDefinition currentConditionItemDefinition)
    {
        for (int i = 0; i < taskConditions.Count; i++) {
            var condition = taskConditions[i];
            if (condition == null) continue;

            if (condition.ConditionItemDefinition == currentConditionItemDefinition) {
                return (int)Mathf.Lerp(condition.MinConditionAmount, condition.MaxConditionAmount, GameStageSystem.CalculateGameStagePercent());
            }
        }

        return 0;
    }

    public ItemDefinition GetRandomConditionItemDefinition(ItemDefinition currentRewardItemDefinition)
    {
        var conditions = GetExcludedTasksList(currentRewardItemDefinition);
        var index = UnityEngine.Random.Range(0, conditions.Count);
        var randomCondition = conditions[index];

        if (randomCondition == null) {
            Debug.LogError($"TaskCondition is not valid by index {index}!");
            return null;
        }

        return randomCondition.ConditionItemDefinition;
    }

    private List<ExtractionTaskEntry> GetExcludedTasksList(ItemDefinition currentRewardItemDefinition)
    {
        var conditions = taskConditions.ToList();

        for (int i = conditions.Count - 1; i >= 0; i--) {
            var condition = conditions[i];
            if (condition == null) continue;

            if (condition.ConditionItemDefinition == currentRewardItemDefinition) {
                conditions.RemoveAt(i);
                break;
            }
        }

        return conditions;
    }
}