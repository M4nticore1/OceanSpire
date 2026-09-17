using UnityEngine;

public enum ItemStackId
{
    Population,
    Electricity,
    Food,
    Wood,
    Stone,
    Scrap,
    Plastic,
    Ingredients,
    Weapon,
}

[CreateAssetMenu(fileName = "ItemStackDefinition", menuName = "Scriptable Objects/ItemStackDefinition")]
public class ItemStackDefinition : ScriptableObject
{
    [SerializeField] private ItemStackId stackId;
    public ItemStackId StackId => stackId;

    [SerializeField] private LocalizationItem nameLocalizationItem;
    public LocalizationItem NameLocalizationItem => nameLocalizationItem;

    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    [SerializeField] private Sprite stackIcon;
    public Sprite Icon => stackIcon;
}