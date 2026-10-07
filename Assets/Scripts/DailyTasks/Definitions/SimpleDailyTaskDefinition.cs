using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SimpleDailyTaskDefinition", menuName = "Daily Tasks/SimpleDailyTaskDefinition")]
public class SimpleDailyTaskDefinition : DailyTaskDefinition
{
    [Header("Simple Definition")]
    [SerializeField, FormerlySerializedAs("conditionAmount")] private int minConditionAmount = 0;
    public int MinConditionAmount => minConditionAmount;

    [SerializeField] private int maxConditionAmount = 0;
    public int MaxConditionAmount => maxConditionAmount;

    [SerializeField] private Sprite conditionImage;
    public Sprite ConditionImage => conditionImage;

    public override ItemInstance GetRandomReward()
    {
        var index = UnityEngine.Random.Range(0, RandomRewards.Length);

        var randomReward = RandomRewards[index];
        if (randomReward == null) return null;

        var reward = new ItemInstance(randomReward.Definition);
        var gameStage = GameStageSystem.CalculateGameStagePercent();
        var amount = (int)(Mathf.Lerp(randomReward.MinAmount, randomReward.MinAmount, gameStage));
        reward.SetAmount(amount);

        if (reward == null) {
            Debug.LogError($"Random Reward is not valid at {this}!");
            return null;
        }

        return reward;
    }

    public override int GetConditionByGameStage(DailyTaskInstance taskInstance)
    {
        return (int)Mathf.Lerp(minConditionAmount, MaxConditionAmount, GameStageSystem.CalculateGameStagePercent());
    }

    public override DailyTaskInstance CreateInstance(ItemInstance reward, int progress = 0, bool completed = false)
    {
        return new SimpleDailyTaskInstance(this, reward, progress, completed);
    }
}