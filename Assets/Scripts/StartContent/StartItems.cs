using UnityEngine;

public class StartItems : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ItemInstance[] startItems;

    public void CollectItems()
    {
        foreach (var item in startItems) {
            int id = item.Definition.ItemId;
            int amount = item.Amount;
            cityStorage.Inventory.AddItem(id, amount);
        }
    }
}