using UnityEngine;

public class ItemsLoader : WorldLoader
{
    public static ItemsLoader Instance { get; private set; }

    [SerializeField] private CityStorage cityStorage;

    [SerializeField] private StartItems startItems;

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
        if (data != null && data.CityInventory != null) {
            for (int i = 0; i < data.CityInventory.Length; i++) {
                ItemData itemData = data.CityInventory[i];
                cityStorage.Inventory.AddItem(itemData.Id, itemData.Amount);
            }
        }
        else {
            startItems.CollectItems();
        }
    }
}