using UnityEngine;

public class StartItems : MonoBehaviour
{
    [SerializeField] private ItemInstance[] startItems = null;

    public void CollectItems()
    {
        foreach (var item in startItems) {
            int id = item.Definition.ItemId;
            int amount = item.Amount;
            CityStorage.Instance.Inventory.AddItem(id, amount);
        }
    }
}
