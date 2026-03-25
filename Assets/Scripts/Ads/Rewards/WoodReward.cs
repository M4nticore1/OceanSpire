using UnityEngine;

[CreateAssetMenu(fileName = "WoodReward", menuName = "Scriptable Objects/Wood Reward")]
public class WoodReward : AdReward
{
    private CityStorage cityStorage;

    public override void GrantReward()
    {
        cityStorage = FindAnyObjectByType<CityStorage>();

        int woodId = ItemsList.Instance.itemsDict["wood"].ItemId;
        cityStorage.Inventory.AddItemAmount(woodId, 1000);
    }
}
