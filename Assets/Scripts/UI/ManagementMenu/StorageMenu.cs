using System.Collections.Generic;
using UnityEngine;

public class StorageMenu : ManagementMenu
{
    [SerializeField] private CityStorage cityStorage;
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    private List<ResourceWidget> spawnedWidgets = new List<ResourceWidget>();

    protected override void CreateWidgets()
    {
        int count = ItemsList.Instance.Items.Length;

        for (int i = 0; i < count; i++) {
            ItemInstance amountItem = cityStorage.Inventory.items[i].item;
            ItemData amountItemData = amountItem.ItemData;

            if (amountItemData.ItemCategory == ItemCategory.Society)
                continue;

            ItemInstance maxAmountItem = cityStorage.Inventory.items[i].maxAmountItem;
            ItemData maxAmountItemData = maxAmountItem.ItemData;

            ItemCategory itemCategory = amountItemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, lists[(int)itemCategory - 1].transform);
            storageResourceWidget.Init(amountItem, maxAmountItem);
            spawnedWidgets.Add(storageResourceWidget);
        }
    }
}
