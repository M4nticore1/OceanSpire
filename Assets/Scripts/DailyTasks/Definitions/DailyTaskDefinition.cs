using UnityEngine;

public abstract class DailyTaskDefinition : ScriptableObject
{
    [Header("Reward")]
    [SerializeField] private RandomItemInstance[] randomRewards;
    public RandomItemInstance[] RandomRewards => randomRewards;

    [Header("UI")]
    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    public abstract int GetConditionAmount(ItemDefinition currentRewardItemDefinition);

    public abstract ItemInstance GetRandomReward();

    public abstract DailyTaskInstance CreateInstance(ItemInstance reward, int progress = 0, bool completed = false);
}