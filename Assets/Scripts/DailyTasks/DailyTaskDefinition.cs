using UnityEngine;

public enum DailyTaskId
{
    time_wood_easy,
    time_stone_easy,
    time_scrap_easy,
    time_plastic_easy,
    building_wood_medium,
    building_stone_medium,
    building_scrap_medium,
    building_plastic_medium,
    raid_wood_hard,
    raid_stone_hard,
    raid_scrap_hard,
    raid_plastic_hard,
}

[CreateAssetMenu(fileName = "DailyTaskDefinition", menuName = "Daily Tasks/DailyTaskDefinition")]
public class DailyTaskDefinition : ScriptableObject
{
    //[SerializeField] private DailyTaskId taskId;
    //public DailyTaskId TaskId => taskId;

    [Header("Condition")]
    [SerializeField] private int conditionAmount = 0;
    public int ConditionAmount => conditionAmount;

    [SerializeField] private Sprite conditionImage;
    public Sprite ConditionImage => conditionImage;

    [SerializeField] private LocalizationItem conditionLocalizationItem;
    public LocalizationItem ConditionLocalizationItem => conditionLocalizationItem;

    [Header("Reward")]
    [SerializeField] private RandomItemInstance[] randomRewards;
    public RandomItemInstance[] RandomRewards => randomRewards;

    //[SerializeField] private ItemInstance reward;
    //public ItemInstance Reward => reward;

    [Header("Other")]
    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    public ItemInstance GetRandomReward()
    {
        var index = UnityEngine.Random.Range(0, randomRewards.Length);

        var randomReward = randomRewards[index];
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
}