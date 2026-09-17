using UnityEngine;

public enum BoatIdEnum
{
    BasicBoat,
    Raft,
    RaidBoat
}

[CreateAssetMenu(fileName = "BoatDefinition", menuName = "Scriptable Objects/BoatDefinition")]
public class BoatDefinition : ScriptableObject
{
    [SerializeField] private BoatIdEnum boatId = BoatIdEnum.BasicBoat;
    public BoatIdEnum BoatId => boatId;

    [SerializeField] private float boatSpeed = 1;
    public float BoatSpeed => boatSpeed;

    [SerializeField] private LocalizationItem nameLocalizationItem;
    public LocalizationItem NameLocalizationItem => nameLocalizationItem;

    [SerializeField] private LocalizationItem descriptionLocalizationItem;
    public LocalizationItem DescriptionLocalizationItem => descriptionLocalizationItem;

    [SerializeField] private Sprite sprite;
    public Sprite Sprite => sprite;
}
