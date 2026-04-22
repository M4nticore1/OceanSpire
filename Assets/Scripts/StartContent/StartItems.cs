using UnityEngine;

public class StartItems : MonoBehaviour
{
    [SerializeField] private ItemInstance[] startItems = null;

    public void CollectItems()
    {
        foreach (var item in startItems) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            CityStorage.Instance.Inventory.AddItemAmount(id, amount);
        }
    }
}
