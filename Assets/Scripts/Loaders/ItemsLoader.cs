using UnityEngine;

public class ItemsLoader : Loader
{
    public static ItemsLoader Instance { get; private set; }
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
        if (data != null && data.Items != null) {
            for (int i = 0; i < data.Items.Length; i++) {
                ItemData itemData = data.Items[i];
                CityStorage.Instance.Inventory.AddItem(itemData.Id, itemData.Amount);
            }
        }
        else {
            startItems.CollectItems();
        }
    }
}