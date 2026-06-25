using UnityEngine;
using System.Collections.Generic;

public class ItemRewardInstance : RewardInstance
{
    public ItemAdRewardDefinition ItemRewardDefinition { get; private set; }

    public ItemRewardInstance(ItemAdRewardDefinition data, int amount) : base(data, amount)
    {
        ItemRewardDefinition = data;
    }

    protected override void OnRewardRecieved()
    {
        base.OnRewardRecieved();

        int id = ItemRewardDefinition.Definition.ItemId;
        CityStorage.Instance.Inventory.AddItem(id, Amount);
    }

    public void SetAmountPercent(float percent)
    {
        SetAmount((int)Mathf.Lerp(ItemRewardDefinition.MinAmount, ItemRewardDefinition.MaxAmount, percent));
    }

    public void GenerateAmount()
    {
        int minAmount = ItemRewardDefinition.MinAmount;
        int maxAmount = ItemRewardDefinition.MaxAmount;
        SetAmount(Random.Range(minAmount, maxAmount));
    }

    public override RewardInstanceData CreateData()
    {
        return new RewardInstanceData() {
            Id = (int)Definition.RewardId,
            Amount = Amount,
            Collected = IsCollected,
        };
    }
}