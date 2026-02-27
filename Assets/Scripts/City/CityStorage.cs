using Unity.Mathematics;
using UnityEngine;

public class CityStorage : MonoBehaviour
{
    [SerializeField] private Inventory inventory = null;

    private void OnEnable()
    {
        inventory.onChangedItemAmount += OnItemAmountChanged;
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

    private void OnClickedProductionModule(ProductionModule module)
    {
        int id = module.produceItem.produceItem.ItemData.ItemId;
        int amount = module.producedItem.Amount;
        int amountToTake = math.min(amount, inventory.itemsDict[id].maxAmount);

        inventory.AddItem(id, amountToTake);
        module.RemoveItemAmount(amountToTake);
    }

    private void OnBoatUnloadedItem(int id, int amount)
    {
        inventory.AddItem(id, amount);
    }
}
