using UnityEngine;
using UnityEngine.Serialization;

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
    Pipe = 14,
    Crowbar = 15,
    Hammer = 16,
    Hatchet = 17,
    KitchenKnife = 18,
    Machete = 19,
    FireAxe = 20,
    SledgeHammer = 21,
}

public enum ItemCategory
{
    Society,
    Resource,
    Food,
    Weapon
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private ItemID itemId = ItemID.Population;
    public ItemID ItemId => itemId;

    [SerializeField] private ItemCategory itemCategory = ItemCategory.Society;
    public ItemCategory ItemCategory => itemCategory;

    [SerializeField] private ItemStackEnum stack = ItemStackEnum.Population;
    public ItemStackEnum Stack => stack;

    [SerializeField] private float weight = 0;
    public float Weight => weight;

    [SerializeField, FormerlySerializedAs("localizationItem")] private LocalizationItem nameLocalizationItem;
    public LocalizationItem NameLocalizationItem => nameLocalizationItem;

    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    [SerializeField] private Sprite itemIcon = null;
    public Sprite ItemIcon => itemIcon;

    [SerializeField] private bool showInStorage = true;
    public bool ShowInStorage => showInStorage;

    public virtual ItemInstance CreateInstance()
    {
        return new ItemInstance(this);
    }
}
