using System.Collections.Generic;
using UnityEngine;

public abstract class AdRewardData : ScriptableObject
{
    [SerializeField] protected Sprite rewardIcon;
    public Sprite RewardIcon => rewardIcon;

    [SerializeField] protected LocalizationItem rewardDescryptionLocalization;
    public LocalizationItem RewardDescryptionLocalization => rewardDescryptionLocalization;

    [SerializeField] protected LocalizationItem receievedRewardLocalization;
    public LocalizationItem ReceievedRewardLocalization => receievedRewardLocalization;

    public abstract AdRewardInstance CreateInstance();
}
