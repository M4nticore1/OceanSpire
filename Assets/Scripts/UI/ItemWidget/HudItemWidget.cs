using System.Collections;
using UnityEngine;

public class HudItemWidget : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ItemWidget itemWidget;

    private void Start()
    {
        StartCoroutine(UpdateItemWidgetDelay());
    }

    private void UpdateItemWidget()
    {
        var item = cityStorage.Inventory.GetInventoryItem(itemWidget.ItemDefinition.ItemId);
        if (item == null) {
            Debug.LogError($"[{nameof(HudItemWidget)}] Item is not valid at {this}!");
            return;
        }

        var stack = item.Stack;
        if (stack == null) {
            Debug.LogError($"[{nameof(HudItemWidget)}] Stack is not valid at {this}!");
            return;
        }

        itemWidget.AddAmount(item);
        itemWidget.SetLimit(stack);
    }

    private IEnumerator UpdateItemWidgetDelay()
    {
        yield return new WaitForEndOfFrame();

        UpdateItemWidget();
    }
}