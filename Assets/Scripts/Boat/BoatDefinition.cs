using UnityEngine;

public enum BoatIdEnum
{
    BasicBoat,
    Raft,
    RaidBoat
}

[CreateAssetMenu(fileName = "BoatData", menuName = "Boat/BoatData")]
public class BoatDefinition : ScriptableObject
{
    [SerializeField] private BoatIdEnum boatId = BoatIdEnum.BasicBoat;
    public BoatIdEnum BoatId => boatId;

    [SerializeField] private string boatIdName = "";
    public string BoatIdName => boatIdName;

    [SerializeField] private float boatSpeed = 1;
    public float BoatSpeed => boatSpeed;

    [SerializeField] private LocalizationItem nameLocalization;
    public LocalizationItem NameLocalization => nameLocalization;

    public const float healthDrainInterval = 10f;
    public const float healthDisplayThreshold = 0.25f;
}
