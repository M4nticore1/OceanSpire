using UnityEngine;

[CreateAssetMenu(fileName = "DailyTaskDefinition", menuName = "Daily Tasks/DailyTaskDefinition")]
public class DailyTaskDefinition : ScriptableObject
{
    [Header("Condition")]
    [SerializeField] private int conditionAmount = 0;
    public int ConditionAmount => conditionAmount;

    [SerializeField] private Sprite conditionImage;
    public Sprite ConditionImage => conditionImage;

    [SerializeField] private LocalizationItem conditionLocalizationItem;
    public LocalizationItem ConditionLocalizationItem => conditionLocalizationItem;

    [Header("Reward")]
    [SerializeField] private ItemInstance reward;
    public ItemInstance Reward => reward;

    [Header("Other")]
    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;
}