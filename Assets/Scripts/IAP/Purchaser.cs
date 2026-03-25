using UnityEngine;
using UnityEngine.Purchasing;

public class Purchaser : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

    public void OnPurchaseCompleted(Product product)
    {
        switch (product.definition.id) {
            case "com.wismutgames.oceanspire.wood_1000":
                AddWood1000();
                break;
        }
    }

    private void AddWood1000()
    {
        int id = ItemsList.Instance.itemsDict["wood"].ItemId;
        cityStorage.Inventory.AddItemAmount(id, 1000);
    }
}