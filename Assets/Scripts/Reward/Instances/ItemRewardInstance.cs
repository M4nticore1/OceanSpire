using UnityEngine;

public class ItemRewardInstance : RewardInstance
{
    public ItemAdRewardDefinition ItemRewardDefinition { get; private set; }
    private CityStorage cityStorage => CityStorage.Instance;

    public ItemRewardInstance(ItemAdRewardDefinition data, int amount) : base(data, amount)
    {
        ItemRewardDefinition = data;
    }

    protected override void HandleRewardRecieved()
    {
        base.HandleRewardRecieved();

        Debug.Log("HandleRewardRecieved1");
        if (ItemRewardDefinition == null) {
            Debug.LogError($"[{nameof(ItemRewardDefinition)}] Item Reward Definition is not valid!");
            return;
        }

        var itemDefinition = ItemRewardDefinition.ItemDefinition;
        if (itemDefinition == null) {
            Debug.LogError($"[{nameof(CityStorage)}] Item Definition is not valid!");
            return;
        }

        if (cityStorage == null) {
            Debug.LogError($"[{nameof(CityStorage)}] City Storage is not valid!");
            return;
        }

        var id = itemDefinition.ItemId;
        Debug.Log(id + " " + Amount);
        cityStorage.Inventory.AddItemAmount(id, Amount);
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