using UnityEngine;

public class HudItemWidget : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ResourceWidget itemWidget;

    private void Start()
    {
        var item = cityStorage.Inventory.GetInventoryItem(itemWidget.ItemDefinition.ItemId);
        var limit = item.Stack;
        itemWidget.AddAmount(item);
        itemWidget.SetLimit(limit);
    }
}