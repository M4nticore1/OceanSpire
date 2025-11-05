using UnityEngine;

public enum ItemID
{
    Population,
    Food,
    Electricity,
    Wood,
    Stone,
    Metal,
    Plastic,
}

//[System.Serializable]
public enum ItemCategory
{
    Society,
    Building,
    Crafting,
    Weapon,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private ItemID itemId = ItemID.Population;
    public int ItemId => (int)itemId;
    public string itemIdName = "";
    public string itemName = "";
    public Sprite itemIcon = null;
    public ItemCategory itemCategory = ItemCategory.Society;
}
