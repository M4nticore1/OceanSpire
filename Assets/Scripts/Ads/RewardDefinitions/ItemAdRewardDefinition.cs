using UnityEngine;

[CreateAssetMenu(fileName = "ItemRewardData", menuName = "Ads Reward Definitions/reward_item")]
public class ItemAdRewardDefinition : AdRewardDefinition
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
