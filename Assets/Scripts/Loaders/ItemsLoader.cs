using UnityEngine;

public class ItemsLoader : MonoBehaviour
{
    private void Awake()
    {
        WorldData worldData = WorldSaveManager.Instance.currentSaveWorldData;
        LoadItems(worldData);
    }

    private void LoadItems(WorldData saveData)
    {
        if (saveData != null) {

        }
        else {
            foreach (ItemData data in ItemsList.Instance.Items) {
                int id = data.ItemId;
                CityStorage.instance.Inventory.TryAddNewItem(id);
            }
        }
    }
}
