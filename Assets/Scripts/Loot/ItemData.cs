using UnityEngine;

public enum ItemID
{
    Population = 0,
    Electricity = 1,
    Food = 2,
    Wood = 3,
    Stone = 4,
    Scrap = 5,
    Plastic = 6,
    Potato = 7,
    Tomato = 8,
    Cucumber = 9,
    Carrot = 10,
    Cabbage = 11,
    Hands = 12,
    BaseballBat = 13,
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

    [SerializeField] private ItemCategory itemCategory = ItemCategory.Society;
    public ItemCategory ItemCategory => itemCategory;

    //[SerializeField] private string itemIdName = "";
    //public string itemKey => itemIdName;

    [SerializeField] private float weight = 0;
    public float Weight => weight;

    [SerializeField] private LocalizationItem localizationItem;
    public LocalizationItem LocalizationItem => localizationItem;

    [SerializeField] private Sprite itemIcon = null;
    public Sprite ItemIcon => itemIcon;

    [SerializeField] private bool showInStorage = true;
    public bool ShowInStorage => showInStorage;
}
