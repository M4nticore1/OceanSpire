using UnityEngine;
using System.Collections.Generic;

public class ItemRewardInstance : RewardInstance
{
    public ItemAdRewardDefinition ItemRewardDefinition { get; private set; }
    public int Amount { get; private set; } = 0;

    public ItemRewardInstance(ItemAdRewardDefinition data, int amount) : base(data)
    {
        ItemRewardDefinition = data;
        Amount = amount;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "itemName", LocalizationManager.Instance.GetText(ItemRewardDefinition.Definition.NameLocalization).ToLower() },
            { "amount", Amount.ToString()},
        };
    }

    protected override void OnRewardRecieved()
    {
        base.OnRewardRecieved();

        int id = ItemRewardDefinition.Definition.ItemId;
        CityStorage.Instance.Inventory.AddItem(id, Amount);
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
    }

    public void SetAmountPercent(float percent)
    {
        Amount = (int)Mathf.Lerp(ItemRewardDefinition.MinAmount, ItemRewardDefinition.MaxAmount, percent);
    }

    public void GenerateAmount()
    {
        int minAmount = ItemRewardDefinition.MinAmount;
        int maxAmount = ItemRewardDefinition.MaxAmount;
        Amount = Random.Range(minAmount, maxAmount);
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