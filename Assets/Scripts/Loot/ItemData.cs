using UnityEngine;

public enum ItemID
{
    Population = 0,
    Electricity = 1,
    Food = 2,
    Water = 3,
    Wood = 4,
    Stone = 5,
    Metal = 6,
    Plastic = 7,
    Potato = 8,
    Hands = 9,
    BaseballBat = 10,
}

//[System.Serializable]
public enum ItemCategory
{
    Society,
    Building,
    Food,
    Weapon
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private ItemID itemId = ItemID.Population;
    public int ItemId => (int)itemId;

    [SerializeField] private string itemIdName = "";
    public string itemKey => itemIdName;

    [SerializeField] private string itemName = "";
    public string ItemName => itemName;

    [SerializeField] private float weight = 0;
    public float Weight => weight;

    [SerializeField] private ItemCategory itemCategory = ItemCategory.Society;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private LocalizationItem localizationItem;
    public LocalizationItem LocalizationItem => localizationItem;

    [SerializeField] private Sprite itemIcon = null;
    public Sprite ItemIcon => itemIcon;

    [SerializeField] private bool showInStorage = true;
    public bool ShowInStorage => showInStorage;
}
