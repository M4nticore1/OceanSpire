using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WoodReward", menuName = "Scriptable Objects/Wood Reward")]
public class ItemReward : AdReward, ILocalizable
{
    private Dictionary<string, string> localization;
    public Dictionary<string, string> Localization => localization;
    private CityStorage cityStorage;

    [SerializeField] private ItemData itemData;
    [SerializeField] private int minAmount;
    [SerializeField] private int maxAmount;

    public override void Init()
    {
        localization = new Dictionary<string, string>() {
            {"itemName", itemData.ItemName },
            {"minAmount", minAmount.ToString() },
            {"maxAmount", minAmount.ToString() }
        };
    }

    protected override void OnRewardRecieved()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        int woodId = ItemsList.Instance.itemsDict["wood"].ItemId;
        int amount = Random.Range(minAmount, maxAmount);

        cityStorage.Inventory.AddItemAmount(woodId, amount);
    }
}
