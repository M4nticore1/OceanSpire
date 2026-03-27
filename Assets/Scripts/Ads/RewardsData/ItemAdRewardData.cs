using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemRewardData", menuName = "Ads Reward Data/reward_item")]
public class ItemAdRewardData : AdRewardData
{
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;

    [SerializeField] private int minAmount;
    public int MinAmount => minAmount;

    [SerializeField] private int maxAmount;
    public int MaxAmount => maxAmount;

    public override AdRewardInstance CreateInstance()
    {
        return new ItemAdRewardInstance(this);
    }
}
