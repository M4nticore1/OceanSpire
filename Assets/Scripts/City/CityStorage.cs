using Unity.Mathematics;
using UnityEngine;

public class CityStorage : MonoBehaviour
{
    [SerializeField] private Inventory inventory = null;
    public Inventory Inventory => GetInventory();

    private void OnEnable()
    {
        inventory.onChangedItemAmount += OnItemAmountChanged;
        inventory.onChangedItemMaxAmount += OnItemMaxAmountChanged;
        EventBus.onClickedProductionModule += OnClickedProductionModule;
        EventBus.onBoatUnloadedItem += OnBoatUnloadedItem;
    }

    private void OnDisable()
    {
        inventory.onChangedItemAmount -= OnItemAmountChanged;
        EventBus.onClickedProductionModule -= OnClickedProductionModule;
        EventBus.onBoatUnloadedItem -= OnBoatUnloadedItem;
    }

    // Inventory
    private void OnItemAmountChanged(ItemInstance item)
    {
        EventBus.InvokeMainStorageAmountChanged(item);
    }

    private void OnItemMaxAmountChanged(StorageItem item)
    {
        EventBus.InvokeMainStorageMaxAmountChanged(item);
    }

    private void OnClickedProductionModule(ProductionModule module)
    {
        int id = module.produceItem.produceItem.ItemData.ItemId;
        int amount = module.producedItem.Amount;
        int amountToTake = math.min(amount, inventory.itemsDict[id].maxAmount);

        inventory.AddItemAmount(id, amountToTake);
        module.RemoveItemAmount(amountToTake);
    }

    private void OnBoatUnloadedItem(int id, int amount)
    {
        inventory.AddItemAmount(id, amount);
    }

    private Inventory GetInventory()
    {
        return inventory;
    }
}
