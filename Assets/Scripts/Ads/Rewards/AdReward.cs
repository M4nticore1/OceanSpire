using UnityEngine;
using UnityEngine.UI;

public abstract class AdReward : ScriptableObject
{
    [SerializeField] protected Image rewardIcon;
    [SerializeField] protected LocalizationItem rewardNameLocalization;
    [SerializeField] protected LocalizationItem rewardDescryptionLocalization;

    public abstract void GrantReward();
}
