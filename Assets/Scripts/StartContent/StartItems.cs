using UnityEngine;

public class StartItems : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ItemInstance[] startItems = null;

    private void Start()
    {
        foreach (var item in startItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            cityStorage.Inventory.AddItemAmount(id, amount);
        }
    }
}
