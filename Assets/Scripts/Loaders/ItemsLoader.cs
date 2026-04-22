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
        if (data != null) {

        }
        else {
            startItems.CollectItems();
        }
    }
}