using UnityEngine;

public class CityStorageLoader : WorldLoader
{
    public static CityStorageLoader Instance { get; private set; }

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

    protected override void Load(WorldData worldData)
    {
        var cityStorageData = worldData?.CityStorage;

        if (cityStorageData != null) {
            cityStorage.Init(cityStorageData);
        }
        else {
            startItems.CollectItems();
        }      
    }
}