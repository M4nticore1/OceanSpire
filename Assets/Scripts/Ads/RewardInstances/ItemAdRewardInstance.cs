using UnityEngine;

public class ItemAdRewardInstance : AdRewardInstance
{
    private ItemAdRewardData itemRewardData;
    private CityStorage cityStorage;
    public int amount { get; private set; } = 0;

    public ItemAdRewardInstance(ItemAdRewardData data) : base(data)
    {
        itemRewardData = data;
        cityStorage = Object.FindAnyObjectByType<CityStorage>();

        GenerateAmount();
        AssignPlaceHoldersLocalization();
    }

    protected override void OnRewardRecieved()
    {
        int woodId = itemRewardData.ItemData.ItemId;
        cityStorage.Inventory.AddItemAmount(woodId, amount);
    }

    protected override void AssignPlaceHoldersLocalization()
    {
        localization.Add("itemName", LocalizationManager.Instance.GetText(itemRewardData.ItemData.LocalizationItem));
        localization.Add("amount", amount.ToString());
    }

    private void GenerateAmount()
    {
        int minAmount = itemRewardData.MinAmount;
        int maxAmount = itemRewardData.MaxAmount;
        amount = Random.Range(minAmount, maxAmount);
    }
}
