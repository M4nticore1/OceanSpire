using UnityEngine;
using System.Collections.Generic;

public class ItemAdRewardInstance : AdRewardInstance
{
    public ItemAdRewardDefinition itemRewardData { get; private set; }
    public int amount { get; private set; } = 0;

    public Dictionary<string, string> Localization;

    public ItemAdRewardInstance(ItemAdRewardDefinition data) : base(data)
    {
        itemRewardData = data;

        GenerateAmount();
    }

    public override Dictionary<string, string> GetLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "itemName", LocalizationManager.Instance.GetText(itemRewardData.ItemData.LocalizationItem).ToLower() },
            { "amount", amount.ToString()},
        };
    }

    protected override void OnRewardRecieved()
    {
        int woodId = itemRewardData.ItemData.ItemId;
        CityStorage.Instance.Inventory.AddItemAmount(woodId, amount);
    }

    private void GenerateAmount()
    {
        int minAmount = itemRewardData.MinAmount;
        int maxAmount = itemRewardData.MaxAmount;
        amount = Random.Range(minAmount, maxAmount);
    }
}