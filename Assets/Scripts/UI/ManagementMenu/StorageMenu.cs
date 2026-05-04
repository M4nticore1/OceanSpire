using System.Collections.Generic;
using UnityEngine;

public class StorageMenu : ManagementMenu
{
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    private List<ResourceWidget> spawnedWidgets = new List<ResourceWidget>();

    protected override void CreateWidgets()
    {
        int count = CityStorage.Instance.Inventory.Items.Count;

        for (int i = 0; i < count; i++) {
            ItemInstance amountItem = CityStorage.Instance.Inventory.Items[i].item;
            ItemDefinition amountItemData = amountItem.ItemData;

            if (!amountItemData.ShowInStorage) continue;

            ItemInstance maxAmountItem = CityStorage.Instance.Inventory.Items[i].maxAmountItem;
            ItemDefinition maxAmountItemData = maxAmountItem.ItemData;

            ItemCategory itemCategory = amountItemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, lists[(int)itemCategory - 1].transform);
            storageResourceWidget.SetAmountItem(amountItem);
            storageResourceWidget.SetMaxAmountItem(maxAmountItem);

            spawnedWidgets.Add(storageResourceWidget);
        }
    }
}