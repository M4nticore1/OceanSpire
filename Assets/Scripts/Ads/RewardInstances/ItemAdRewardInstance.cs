using UnityEngine;
using System.Collections.Generic;

public class ItemAdRewardInstance : AdRewardInstance, ILocalizable
{
    public ItemAdRewardData rewardData { get; private set; }
    private CityStorage cityStorage;
    public int amount { get; private set; } = 0;

    public Dictionary<string, string> Localization => GetPlaceHoldersLocalization();

    public ItemAdRewardInstance(ItemAdRewardData data)
    {
        rewardData = data;
        cityStorage = Object.FindAnyObjectByType<CityStorage>();

        GenerateAmount();
        GetPlaceHoldersLocalization();
    }

    protected override void OnRewardRecieved()
    {
        int woodId = rewardData.ItemData.ItemId;
        cityStorage.Inventory.AddItemAmount(woodId, amount);
    }

    protected Dictionary<string, string> GetPlaceHoldersLocalization()
    {
        return new Dictionary<string, string>()
        {
            { "itemName", LocalizationManager.Instance.GetText(rewardData.ItemData.LocalizationItem).ToLower() },
            { "amount", amount.ToString()},
        };
    }

    private void GenerateAmount()
    {
        int minAmount = rewardData.MinAmount;
        int maxAmount = rewardData.MaxAmount;
        amount = Random.Range(minAmount, maxAmount);
    }
}
