using UnityEngine;

public class ItemsLoader : Loader
{
    public static ItemsLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Duplicate ItemsLoader found in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Load(WorldData data)
    {
        if (data != null) {

        }
        else {
            foreach (var itemData in ItemsList.Instance.Items) {
                int id = itemData.ItemId;
                CityStorage.Instance.Inventory.TryAddNewItem(id);
            }
        }
    }
}