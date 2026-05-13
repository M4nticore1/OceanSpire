using UnityEngine;

public enum RewardId
{
    DailyRewardWood,
    DailyRewardStone,
    DailyRewardScrap,
    DailyRewardPlastic,
}

public abstract class AdRewardDefinition : ScriptableObject
{
    [SerializeField] private RewardId rewardId;
    public RewardId RewardId => rewardId;

    [SerializeField] protected Sprite rewardIcon;
    public Sprite RewardIcon => rewardIcon;

    [SerializeField] protected LocalizationItem rewardNameLocalization;
    public LocalizationItem RewardNameLocalization => rewardNameLocalization;

    [SerializeField] protected LocalizationItem rewardDescryptionLocalization;
    public LocalizationItem RewardDescryptionLocalization => rewardDescryptionLocalization;

    [SerializeField] protected LocalizationItem receievedRewardLocalization;
    public LocalizationItem ReceievedRewardLocalization => receievedRewardLocalization;

    public abstract RewardInstance CreateInstance();
}