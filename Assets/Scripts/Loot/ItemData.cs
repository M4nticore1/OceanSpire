using UnityEngine;

//[System.Serializable]
public enum ItemCategory
{
    Society,
    Building,
    Crafting,
    Weapon,
}

[CreateAssetMenu(fileName = "ResourceData", menuName = "Scriptable Objects/ResourceData")]
public class ItemData : ScriptableObject
{
    public int itemId;
    public string itemIdName;
    public string itemName;
    public Sprite itemIcon;
    public ItemCategory itemCategory;
}
