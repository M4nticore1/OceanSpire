using UnityEngine;

public enum ItemID
{
    Population,
    Electricity,
    Food,
    Water,
    Wood,
    Stone,
    Metal,
    Plastic,
    Potato,
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
}
