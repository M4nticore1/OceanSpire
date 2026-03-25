using UnityEngine;

[CreateAssetMenu(fileName = "WoodReward", menuName = "Scriptable Objects/Wood Reward")]
public class WoodReward : AdReward
{
    private CityStorage cityStorage;

    protected override void OnRewardRecieved()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        int woodId = ItemsList.Instance.itemsDict["wood"].ItemId;
        cityStorage.Inventory.AddItemAmount(woodId, 1000);
    }
}
