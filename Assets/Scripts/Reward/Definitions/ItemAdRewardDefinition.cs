using UnityEngine;

[CreateAssetMenu(fileName = "ItemRewardData", menuName = "Ads Reward Definitions/reward_item")]
public class ItemAdRewardDefinition : AdRewardDefinition
{
    [SerializeField] private ItemDefinition itemData;
    public ItemDefinition Definition => itemData;

    [SerializeField] private int minAmount;
    public int MinAmount => minAmount;

    [SerializeField] private int maxAmount;
    public int MaxAmount => maxAmount;

    public override RewardInstance CreateReward()
    {
        return new ItemRewardInstance(this, 0);
    }
}
