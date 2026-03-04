using UnityEngine;

public class ItemsLoader : MonoBehaviour
{
    [SerializeField] private CityStorage cityStorage;

    private void Start()
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
                cityStorage.Inventory.TryAddNewItem(id);
            }
        }
    }
}
