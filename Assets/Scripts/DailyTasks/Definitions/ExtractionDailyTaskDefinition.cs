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
        if (randomReward == null) return null;

        var reward = new ItemInstance(randomReward.Definition);
        var gameStage = GameStageSystem.CalculateGameStagePercent();
        var amount = (int)(Mathf.Lerp(randomReward.MinAmount, randomReward.MaxAmount, gameStage));
        reward.SetAmount(amount);

        if (reward == null) {
            Debug.LogError($"Random Reward is not valid at {this}!");
            return null;
        }

        return reward;
    }

    public override int GetConditionByGameStage(DailyTaskInstance taskInstance)
    {
        if (taskInstance == null) {
            Debug.LogError("TaskInstance is not valid!");
            return 0;
        }

        if (taskInstance.Reward == null) {
            Debug.LogError("TaskInstance Reward is not valid!");
            return 0;
        }

        var conditions = taskConditions.ToList();

        for (int i = conditions.Count - 1; i >= 0; i--) {
            var condition = conditions[i];
            if (condition == null) continue;

            if (condition.ConditionItemDefinition == taskInstance.Reward.Definition) {
                conditions.RemoveAt(i);
                break;
            }
        }

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

        return (int)Mathf.Lerp(finalCondition.MinConditionAmount, finalCondition.MaxConditionAmount, GameStageSystem.CalculateGameStagePercent());
    }

    public override DailyTaskInstance CreateInstance(ItemInstance reward, int progress = 0, bool completed = false)
    {
        return new ExtractionDailyTaskInstance(this, reward, progress, completed);
    }

    public ItemDefinition GetRandomConditionItemDefinition()
    {
        var index = UnityEngine.Random.Range(0, taskConditions.Count);

        var condition = taskConditions[index];
        if (condition == null) {
            Debug.LogError($"TaskCondition is not valid by index {index}!");
            return null;
        }

        return condition.ConditionItemDefinition;
    }
}