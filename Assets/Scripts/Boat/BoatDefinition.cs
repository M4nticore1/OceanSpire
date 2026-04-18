using UnityEngine;

public enum BoatIdEnum
{
    BasicBoat
}

[CreateAssetMenu(fileName = "BoatData", menuName = "Boat/BoatData")]
public class BoatDefinition : ScriptableObject
{
    [SerializeField] private BoatIdEnum boatId = BoatIdEnum.BasicBoat;
    public int BoatId => (int)boatId;

    [SerializeField] private string boatIdName = "";
    public string BoatIdName => boatIdName;

    [SerializeField] private LocalizationItem nameLocalization;
    public LocalizationItem NameLocalization => nameLocalization;

    public const float healthDrainInterval = 10f;
    public const float healthDisplayThreshold = 0.25f;
}
