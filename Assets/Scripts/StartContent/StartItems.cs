using UnityEngine;

public class StartItems : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ItemInstance[] startItems;

    public void CollectItems()
    {
        foreach (var item in startItems) {
            var id = item.Definition.ItemId;
            var amount = item.Amount;
            cityStorage.Inventory.AddItem(id, amount);
        }
    }
}