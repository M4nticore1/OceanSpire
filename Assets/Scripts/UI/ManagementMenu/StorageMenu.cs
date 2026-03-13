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
            ItemInstance item = cityStorage.Inventory.items[i].item;
            ItemData itemData = item.ItemData;

            if (itemData.ItemCategory == ItemCategory.Society)
                continue;

            ItemCategory itemCategory = itemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, lists[(int)itemCategory - 1].transform);
            storageResourceWidget.Init(item);
            spawnedWidgets.Add(storageResourceWidget);
        }
    }
}
