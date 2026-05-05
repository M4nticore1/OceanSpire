using UnityEngine;

public abstract class AdRewardDefinition : ScriptableObject
{
    [SerializeField] protected Sprite rewardIcon;
    public Sprite RewardIcon => rewardIcon;

    [SerializeField] protected LocalizationItem rewardNameLocalization;
    public LocalizationItem RewardNameLocalization => rewardNameLocalization;

    [SerializeField] protected LocalizationItem rewardDescryptionLocalization;
    public LocalizationItem RewardDescryptionLocalization => rewardDescryptionLocalization;

    [SerializeField] protected LocalizationItem receievedRewardLocalization;
    public LocalizationItem ReceievedRewardLocalization => receievedRewardLocalization;

    public abstract AdRewardInstance CreateInstance();
}