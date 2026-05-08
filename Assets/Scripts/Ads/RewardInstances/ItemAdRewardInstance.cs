using UnityEngine;
using System.Collections.Generic;

public class ItemAdRewardInstance : AdRewardInstance
{
    public ItemAdRewardDefinition ItemRewardDefinition { get; private set; }
    public int Amount { get; private set; } = 0;

    public ItemAdRewardInstance(ItemAdRewardDefinition data) : base(data)
    {
        ItemRewardDefinition = data;
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "itemName", LocalizationManager.Instance.GetText(ItemRewardDefinition.ItemData.NameLocalization).ToLower() },
            { "amount", Amount.ToString()},
        };
    }

    protected override void OnRewardRecieved()
    {
        int id = ItemRewardDefinition.ItemData.ItemId;
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
}